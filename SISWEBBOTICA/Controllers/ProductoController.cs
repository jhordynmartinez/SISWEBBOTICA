using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.Services;
using SISWEBBOTICA.ViewModels;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SISWEBBOTICA.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        // --- INICIO DE LA CORRECCIÓN ESTRUCTURAL ---
        private readonly IProductoRepository _productoRepo;
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IUnidadMedidaRepository _unidadMedidaRepo;

        public ProductoController(
            IProductoRepository productoRepo,
            ICategoriaRepository categoriaRepo,
            IUnidadMedidaRepository unidadMedidaRepo)
        {
            _productoRepo = productoRepo;
            _categoriaRepo = categoriaRepo;
            _unidadMedidaRepo = unidadMedidaRepo;
        }
        // --- FIN DE LA CORRECCIÓN ESTRUCTURAL ---

        public async Task<IActionResult> Index(string busqueda, string filtroStock)
        {
            ViewData["BusquedaActual"] = busqueda;
            ViewData["FiltroStockActual"] = filtroStock;
            var productos = await _productoRepo.GetProductosAsync(busqueda, filtroStock);
            return View(productos);
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductoVM
            {
                Producto = new Producto(),
                CategoriasList = new SelectList(await _categoriaRepo.GetAllAsync(), "IdCategoria", "Nombre"),
                UnidadesMedidaList = new SelectList(await _unidadMedidaRepo.GetAllAsync(), "IdUnidadMedida", "Nombre")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoVM viewModel)
        {
            if (await _productoRepo.ProductoDuplicadoExistsAsync(viewModel.Producto.Nombre, viewModel.Producto.Presentacion))
            {
                ModelState.AddModelError("Producto.Nombre", "Ya existe un producto con el mismo nombre y presentación.");
            }

            if (ModelState.IsValid)
            {
                viewModel.Producto.Estado = "Activo";
                await _productoRepo.CreateProductoAsync(viewModel.Producto);
                TempData["SuccessMessage"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            viewModel.CategoriasList = new SelectList(await _categoriaRepo.GetAllAsync(), "IdCategoria", "Nombre", viewModel.Producto.IdCategoria);
            viewModel.UnidadesMedidaList = new SelectList(await _unidadMedidaRepo.GetAllAsync(), "IdUnidadMedida", "Nombre", viewModel.Producto.IdUnidadMedida);
            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _productoRepo.GetProductoByIdAsync(id.Value);
            if (producto == null) return NotFound();

            var viewModel = new ProductoVM
            {
                Producto = producto,
                CategoriasList = new SelectList(await _categoriaRepo.GetAllAsync(), "IdCategoria", "Nombre", producto.IdCategoria),
                UnidadesMedidaList = new SelectList(await _unidadMedidaRepo.GetAllAsync(), "IdUnidadMedida", "Nombre", producto.IdUnidadMedida)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, ProductoVM viewModel)
        {
            if (id != viewModel.Producto.IdProducto) return NotFound();

            if (await _productoRepo.ProductoDuplicadoExistsAsync(viewModel.Producto.Nombre, viewModel.Producto.Presentacion, id))
            {
                ModelState.AddModelError("Producto.Nombre", "Ya existe otro producto con el mismo nombre y presentación.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    viewModel.Producto.Estado = "Activo";
                    await _productoRepo.UpdateProductoAsync(viewModel.Producto);
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _productoRepo.ProductoExistsAsync(viewModel.Producto.IdProducto)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.CategoriasList = new SelectList(await _categoriaRepo.GetAllAsync(), "IdCategoria", "Nombre", viewModel.Producto.IdCategoria);
            viewModel.UnidadesMedidaList = new SelectList(await _unidadMedidaRepo.GetAllAsync(), "IdUnidadMedida", "Nombre", viewModel.Producto.IdUnidadMedida);
            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _productoRepo.GetProductoConVentasByIdAsync(id.Value);
            if (producto == null) return NotFound();

            if (producto.DetallesVenta.Any())
            {
                ViewBag.Error = "No se puede eliminar este producto porque tiene ventas asociadas. Solo puede marcarlo como inactivo.";
            }

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _productoRepo.GetProductoConVentasByIdAsync(id);
            if (producto == null) return NotFound();

            if (producto.DetallesVenta.Any())
            {
                producto.Estado = "Inactivo";
                await _productoRepo.UpdateProductoAsync(producto);
                TempData["SuccessMessage"] = "Producto marcado como inactivo porque tiene ventas asociadas.";
            }
            else
            {
                await _productoRepo.DeleteProductoAsync(id);
                TempData["SuccessMessage"] = "Producto eliminado exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReporteStockBajo()
        {
            var productos = await _productoRepo.GetProductosAsync(null, "bajo");
            return View(productos);
        }

        public async Task<IActionResult> ExportarStockBajo()
        {
            var productos = await _productoRepo.GetProductosAsync(null, "bajo");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("StockBajo");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "Nombre del Medicamento";
                worksheet.Cell(currentRow, 2).Value = "Categoría";
                worksheet.Cell(currentRow, 3).Value = "Stock Actual";

                worksheet.Row(1).Style.Font.Bold = true;

                foreach (var prod in productos)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = prod.Nombre;
                    worksheet.Cell(currentRow, 2).Value = prod.Categoria?.Nombre ?? "N/A"; // Verificación de nulidad
                    worksheet.Cell(currentRow, 3).Value = prod.Stock;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ReporteStockBajo_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        private async Task<bool> ProductoExists(int id)
        {
            return await _productoRepo.ProductoExistsAsync(id);
        }
    }
}