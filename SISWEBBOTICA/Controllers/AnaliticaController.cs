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
    public class AnaliticaController : Controller
    {
        private readonly AppDBContext _context;
        private readonly IMLService _mlService;

        public AnaliticaController(AppDBContext context, IMLService mlService)
        {
            _context = context;
            _mlService = mlService;
        }

        // POST: /Analitica/GenerarTemporalEntrenar
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GenerarTemporalEntrenar(int meses = 6)
        {
            try
            {
                await _mlService.EntrenarConHistorialSinteticoAsync(meses);
                TempData["SuccessMessage"] = "Modelo entrenado con dataset temporal sintético correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al entrenar con dataset temporal: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Analitica
        public async Task<IActionResult> Index()
        {
            // Calcular KPIs del día
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            var ventasHoy = await _context.Ventas
                .Where(v => v.FechaVenta.Date == hoy)
                .ToListAsync();

            var ventasMes = await _context.Ventas
                .Where(v => v.FechaVenta >= inicioMes)
                .ToListAsync();

            var viewModel = new DashboardAnaliticaVM
            {
                VentaTotalHoy = ventasHoy.Sum(v => v.TotalPagar),
                VentaTotalMes = ventasMes.Sum(v => v.TotalPagar),
                ProductosVendidosHoy = ventasHoy.Count,
                TicketsPromedio = ventasHoy.Any() ? ventasHoy.Count : 1
            };

            // Obtener predicciones y alertas
            var predicciones = await _mlService.PredecirDemandaAsync();
            viewModel.Predicciones = predicciones.Take(10).ToList();
            viewModel.AlertasStock = predicciones.Where(p => p.Recomendacion == "Comprar Urgente" || p.Recomendacion == "Comprar").Take(10).ToList();

            // Obtener métricas del último entrenamiento (si existen)
            var metricas = await _mlService.ObtenerMetricasAsync();
            viewModel.MetricRMSE = metricas.RMSE;
            viewModel.MetricRSquared = metricas.RSquared;

            // Productos más vendidos (últimos 30 días)
            viewModel.ProductosMasVendidos = await _mlService.ObtenerProductosMasVendidosAsync(inicioMes.AddMonths(-1), inicioMes);

            // Productos de lenta rotación
            viewModel.ProductosLentaRotacion = await _mlService.ObtenerProductosLentaRotacionAsync();

            return View(viewModel);
        }

        // GET: /Analitica/Predecir
        [HttpPost]
        public async Task<IActionResult> Predecir()
        {
            try
            {
                await _mlService.EntrenarModeloAsync();
                TempData["SuccessMessage"] = "Modelo de IA entrenado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al entrenar: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Analitica/GenerarHistorial
        [HttpPost]
        [Authorize(Roles = "Administrador,Vendedor")]
        public async Task<IActionResult> GenerarHistorial(int meses = 6)
        {
            try
            {
                await _mlService.GenerarHistorialVentasSinteticoAsync(meses);
                TempData["SuccessMessage"] = "Historial sintético generado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al generar historial: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Analitica/DetallePredicciones
        public async Task<IActionResult> DetallePredicciones()
        {
            var predicciones = await _mlService.PredecirDemandaAsync();
            return View(predicciones);
        }

        // GET: /Analitica/ProductosMasVendidos
        public async Task<IActionResult> ProductosMasVendidos(DateTime? inicio, DateTime? fin)
        {
            var fechaInicio = inicio ?? DateTime.Now.AddMonths(-1);
            var fechaFin = fin ?? DateTime.Now;

            var productos = await _mlService.ObtenerProductosMasVendidosAsync(fechaInicio, fechaFin);
            return View(productos);
        }

        // GET: /Analitica/LentaRotacion
        public async Task<IActionResult> LentaRotacion()
        {
            var productos = await _mlService.ObtenerProductosLentaRotacionAsync();
            return View(productos);
        }
    }
}