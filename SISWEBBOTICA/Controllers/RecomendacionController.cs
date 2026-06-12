using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
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
        private readonly AppDBContext _context; // Añadido para resolver placeholders de base de datos

        public RecomendacionController(IMLService mlService, AppDBContext context)
        {
            _mlService = mlService;
            _context = context;
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
                if (idProducto.HasValue)
                {
                    var alternativas = await _mlService.RecomendarAlternativasAsync(idProducto.Value, termino ?? "");
                    viewModel.Alternativas = alternativas;

                    // CORRECCIÓN: Obtener el nombre real del producto original de la base de datos
                    var productoOriginal = await _context.Productos.FindAsync(idProducto.Value);
                    viewModel.ProductoOriginalNombre = productoOriginal != null
                        ? productoOriginal.Nombre
                        : $"Producto ID: {idProducto}";

                    viewModel.ProductoOriginalSinStock = productoOriginal != null && productoOriginal.Stock <= 0;
                }
                else if (!string.IsNullOrEmpty(termino))
                {
                    // Búsqueda por término directo
                    var productoCoincidente = await _context.Productos
                        .FirstOrDefaultAsync(p => EF.Functions.Like(p.Nombre, $"%{termino}%") && p.Estado == "Activo");

                    if (productoCoincidente != null)
                    {
                        var alternativas = await _mlService.RecomendarAlternativasAsync(productoCoincidente.IdProducto, termino);
                        viewModel.Alternativas = alternativas;
                        viewModel.ProductoOriginalNombre = productoCoincidente.Nombre;
                        viewModel.IdProductoSeleccionado = productoCoincidente.IdProducto;
                        viewModel.ProductoOriginalSinStock = productoCoincidente.Stock <= 0;
                    }
                    else
                    {
                        viewModel.Alternativas = new System.Collections.Generic.List<Models.ML.ProductoAlternativoResult>();
                        viewModel.ProductoOriginalNombre = $"No se encontró coincidencia para '{termino}'";
                    }
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
            return RedirectToAction(nameof(Compra));
        }
    }
}