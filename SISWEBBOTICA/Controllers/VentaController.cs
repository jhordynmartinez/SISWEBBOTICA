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

        public async Task<IActionResult> Crear()
        {
            var viewModel = new VentaVM();
            await RecargarDatosParaVista(viewModel);
            return View(viewModel);
        }

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

                    // --- INICIO DE CORRECCIÓN DE SEGURIDAD ---
                    if (clienteDefault == null && model.IdCliente == null)
                    {
                        ModelState.AddModelError("", "No se encontró el cliente 'PÚBLICO GENERAL'. Por favor, créelo o contacte al administrador.");
                        await RecargarDatosParaVista(model);
                        return View(model);
                    }
                    var monedaDefault = await _context.Monedas.FirstOrDefaultAsync();
                    if (monedaDefault == null)
                    {
                        ModelState.AddModelError("", "No hay ninguna moneda configurada en el sistema.");
                        await RecargarDatosParaVista(model);
                        return View(model);
                    }
                    // --- FIN DE CORRECCIÓN DE SEGURIDAD ---

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
                    return RedirectToAction("Crear");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine(ex.ToString()); // Para depurar
                    ModelState.AddModelError("", "Ocurrió un error inesperado al registrar la venta. Revise la consola de depuración para más detalles.");
                    await RecargarDatosParaVista(model);
                    return View(model);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var productos = await _context.Productos
                .Where(p => EF.Functions.Like(p.Nombre, $"%{term}%") || p.CodigoBarras == term)
                .Select(p => new
                {
                    id = p.IdProducto,
                    label = $"{p.Nombre} (Stock: {p.Stock}) - S/ {p.PrecioMenor}",
                    value = p.Nombre,
                    precio = p.PrecioMenor,
                    stock = p.Stock
                })
                .Take(15)
                .ToListAsync();

            return Json(productos);
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