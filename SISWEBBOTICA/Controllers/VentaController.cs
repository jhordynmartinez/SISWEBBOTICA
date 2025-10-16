using ClosedXML.Excel; // <-- Importante: Asegúrate de tener este 'using'
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Usuario> _userManager;

        public VentaController(AppDBContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Venta
        // CORREGIDO: Acepta parámetros de fecha para el filtrado
        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            ViewData["FechaInicio"] = fechaInicio?.ToString("yyyy-MM-dd");
            ViewData["FechaFin"] = fechaFin?.ToString("yyyy-MM-dd");

            var ventasQuery = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta.Date >= fechaInicio.Value.Date);
            }
            if (fechaFin.HasValue)
            {
                ventasQuery = ventasQuery.Where(v => v.FechaVenta.Date <= fechaFin.Value.Date);
            }

            var ventas = await ventasQuery.OrderByDescending(v => v.FechaVenta).ToListAsync();
            return View(ventas);
        }

        // GET: /Venta/Crear
        public async Task<IActionResult> Crear()
        {
            var viewModel = new VentaVM();
            await RecargarDatosParaVista(viewModel);
            return View(viewModel);
        }

        // POST: /Venta/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(VentaVM model)
        {
            if (model.Detalles == null || !model.Detalles.Any())
            {
                ModelState.AddModelError("", "Debe seleccionar al menos un medicamento para registrar la venta.");
            }

            if (!ModelState.IsValid)
            {
                await RecargarDatosParaVista(model);
                return View(model);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var configuracionTienda = await _context.Boticas.FirstOrDefaultAsync();
                    bool permitirStockNegativo = configuracionTienda?.PermitirStockNegativo ?? false;

                    var idsProductos = model.Detalles.Select(d => d.IdProducto).ToList();
                    var productosEnVenta = await _context.Productos
                        .Where(p => idsProductos.Contains(p.IdProducto))
                        .ToDictionaryAsync(p => p.IdProducto, p => p);

                    foreach (var detalleVM in model.Detalles)
                    {
                        if (!productosEnVenta.TryGetValue(detalleVM.IdProducto, out var producto) || (!permitirStockNegativo && producto.Stock < detalleVM.Cantidad))
                        {
                            var nombreProducto = producto?.Nombre ?? "desconocido";
                            ModelState.AddModelError("", $"Stock insuficiente para '{nombreProducto}'. Disponible: {producto?.Stock ?? 0}");

                            await RecargarDatosParaVista(model);
                            return View(model);
                        }
                    }

                    var usuarioActual = await _userManager.GetUserAsync(User);
                    var clienteDefault = await _context.Clientes.FirstOrDefaultAsync(c => c.Nombre == "PÚBLICO GENERAL");

                    if (clienteDefault == null && model.IdCliente == null)
                    {
                        ModelState.AddModelError("", "No se encontró el cliente 'PÚBLICO GENERAL'.");
                        await RecargarDatosParaVista(model);
                        return View(model);
                    }
                    var monedaDefault = await _context.Monedas.FirstOrDefaultAsync();
                    if (monedaDefault == null)
                    {
                        ModelState.AddModelError("", "No hay ninguna moneda configurada.");
                        await RecargarDatosParaVista(model);
                        return View(model);
                    }

                    var venta = new Venta
                    {
                        IdUsuario = usuarioActual.Id,
                        IdCliente = model.IdCliente ?? clienteDefault.IdCliente,
                        IdMoneda = monedaDefault.IdMoneda,
                        NumeroComprobante = await GenerarSiguienteCorrelativo(),
                        TotalPagar = model.Total,
                        FechaVenta = DateTime.Now,
                        CondicionPago = "CONTADO"
                    };
                    _context.Ventas.Add(venta);
                    await _context.SaveChangesAsync();

                    foreach (var detalleVM in model.Detalles)
                    {
                        var producto = productosEnVenta[detalleVM.IdProducto];

                        var detalleVenta = new DetalleVenta
                        {
                            IdVenta = venta.IdVenta,
                            IdProducto = detalleVM.IdProducto,
                            Cantidad = detalleVM.Cantidad,
                            Precio = detalleVM.Precio,
                            Importe = detalleVM.Subtotal,
                            Utilidad = (detalleVM.Precio - producto.PrecioCompra) * detalleVM.Cantidad
                        };
                        _context.DetallesVenta.Add(detalleVenta);

                        producto.Stock -= detalleVM.Cantidad;
                        _context.Productos.Update(producto);
                    }

                    var pago = new Pago
                    {
                        IdVenta = venta.IdVenta,
                        IdMetodoPago = model.IdMetodoPago,
                        Monto = model.Total,
                        FechaPago = DateTime.Now,
                        Referencia = model.ReferenciaPago
                    };
                    _context.Pagos.Add(pago);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Venta N° {venta.IdVenta} registrada correctamente.";
                    return RedirectToAction("Boleta", new { id = venta.IdVenta });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    ModelState.AddModelError("", "Ocurrió un error inesperado al registrar la venta. Revise la consola de depuración para más detalles.");

                    await RecargarDatosParaVista(model);
                    return View(model);
                }
            }
        }

        // GET: /Venta/Boleta/5
        public async Task<IActionResult> Boleta(int id)
        {
            var venta = await _context.Ventas.Include(v => v.Cliente).Include(v => v.Usuario).FirstOrDefaultAsync(v => v.IdVenta == id);
            if (venta == null) return NotFound();

            var detalles = await _context.DetallesVenta.Include(d => d.Producto).Where(d => d.IdVenta == id).ToListAsync();
            var tienda = await _context.Boticas.FirstOrDefaultAsync();

            var viewModel = new BoletaVM { Venta = venta, Detalles = detalles, Tienda = tienda };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2) return Json(new List<object>());

            var productos = await _context.Productos
                .Where(p => EF.Functions.Like(p.Nombre, $"%{term}%") || p.CodigoBarras == term)
                .Select(p => new {
                    id = p.IdProducto,
                    label = $"{p.Nombre} (Stock: {p.Stock}) - S/ {p.PrecioMenor}",
                    value = p.Nombre,
                    precio = p.PrecioMenor,
                    stock = p.Stock
                })
                .Take(15).ToListAsync();

            return Json(productos);
        }

        // --- NUEVO MÉTODO PARA EXPORTAR A EXCEL ---
        public async Task<IActionResult> ExportarVentas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var ventasQuery = _context.Ventas.Include(v => v.Cliente).Include(v => v.Usuario).AsQueryable();

            if (fechaInicio.HasValue) { ventasQuery = ventasQuery.Where(v => v.FechaVenta.Date >= fechaInicio.Value.Date); }
            if (fechaFin.HasValue) { ventasQuery = ventasQuery.Where(v => v.FechaVenta.Date <= fechaFin.Value.Date); }

            var ventas = await ventasQuery.OrderByDescending(v => v.FechaVenta).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Ventas");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "N° Boleta";
                worksheet.Cell(currentRow, 2).Value = "Cliente";
                worksheet.Cell(currentRow, 3).Value = "Fecha";
                worksheet.Cell(currentRow, 4).Value = "Total";
                worksheet.Cell(currentRow, 5).Value = "Vendedor";

                // Estilo para la cabecera
                worksheet.Row(1).Style.Font.Bold = true;

                foreach (var v in ventas)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = v.NumeroComprobante;
                    worksheet.Cell(currentRow, 2).Value = v.Cliente.Nombre;
                    worksheet.Cell(currentRow, 3).Value = v.FechaVenta;
                    worksheet.Cell(currentRow, 4).Value = v.TotalPagar;
                    worksheet.Cell(currentRow, 5).Value = v.Usuario.UserName;
                }

                worksheet.Cell(currentRow + 1, 3).Value = "Total General:";
                worksheet.Cell(currentRow + 1, 4).FormulaA1 = $"=SUM(D2:D{currentRow})";
                worksheet.Cell(currentRow + 1, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow + 1, 4).Style.Font.Bold = true;


                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ReporteVentas_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        private async Task RecargarDatosParaVista(VentaVM model)
        {
            var clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
            model.Clientes = new SelectList(clientes, "IdCliente", "Nombre", model.IdCliente);
            model.MetodosPago = new SelectList(await _context.MetodosPago.ToListAsync(), "IdMetodoPago", "Nombre", model.IdMetodoPago);
        }

        private async Task<string> GenerarSiguienteCorrelativo()
        {
            var ultimaVentaId = await _context.Ventas.MaxAsync(v => (int?)v.IdVenta) ?? 0;
            return $"B001-{(ultimaVentaId + 1).ToString("D8")}";
        }
    }
}