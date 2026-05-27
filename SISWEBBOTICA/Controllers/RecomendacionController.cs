using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.Services;
using SISWEBBOTICA.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize]
    public class RecomendacionController : Controller
    {
        private readonly IMLService _mlService;

        public RecomendacionController(IMLService mlService)
        {
            _mlService = mlService;
        }

        // GET: /Recomendacion/Compra - Optimización de compras para administrador
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Compra(string filtroPrioridad)
        {
            var comprasSugeridas = await _mlService.OptimizarComprasAsync();

            if (!string.IsNullOrEmpty(filtroPrioridad) && filtroPrioridad != "Todas")
            {
                comprasSugeridas = comprasSugeridas.Where(c => c.Prioridad == filtroPrioridad).ToList();
            }

            var viewModel = new RecomendacionVM
            {
                ComprasSugeridas = comprasSugeridas,
                FiltroPrioridad = filtroPrioridad ?? "Todas"
            };

            return View(viewModel);
        }

        // GET: /Recomendacion/Producto - Búsqueda de alternativas para vendedores
        public async Task<IActionResult> Producto(string termino, int? idProducto)
        {
            var viewModel = new RecomendacionVM
            {
                TerminoBusqueda = termino ?? "",
                IdProductoSeleccionado = idProducto
            };

            if (!string.IsNullOrEmpty(termino) || idProducto.HasValue)
            {
                int idProductoBusqueda = idProducto ?? 0;
                
                // Si hay ID de producto, buscar alternativas directas
                if (idProducto.HasValue)
                {
                    var producto = await _mlService.RecomendarAlternativasAsync(idProducto.Value, termino ?? "");
                    viewModel.Alternativas = producto;
                    
                    // Obtener nombre del producto original
                    var productoOriginal = new object(); // Placeholder - en producción obtener de BD
                    viewModel.ProductoOriginalNombre = $"Producto ID: {idProducto}";
                    viewModel.ProductoOriginalSinStock = true;
                }
                else if (!string.IsNullOrEmpty(termino))
                {
                    // Búsqueda por término - encontrar producto y sugerir alternativas
                    // Esto es un placeholder - en producción buscar en la BD
                    viewModel.Alternativas = new System.Collections.Generic.List<Models.ML.ProductoAlternativoResult>();
                }
            }

            return View(viewModel);
        }

        // POST: /Recomendacion/BuscarAlternativas
        [HttpPost]
        public async Task<IActionResult> BuscarAlternativas(int idProducto, string termino)
        {
            var alternativas = await _mlService.RecomendarAlternativasAsync(idProducto, termino);
            return Json(alternativas);
        }

        // GET: /Recomendacion/ExportarCompras
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ExportarCompras()
        {
            var comprasSugeridas = await _mlService.OptimizarComprasAsync();
            
            // Aquí se podría generar un PDF o Excel con la lista de compras
            // Por ahora redirigimos a la vista de compra
            return RedirectToAction(nameof(Compra));
        }
    }
}