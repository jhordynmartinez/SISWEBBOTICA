using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize] // Requiere que el usuario esté logueado para acceder a cualquier parte de este controlador
    public class HomeController : Controller
    {
        private readonly AppDBContext _context;

        public HomeController(AppDBContext context)
        {
            _context = context;
        }

        // Esta será la acción principal de tu Dashboard
        // EN Controllers/HomeController.cs

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardVM();

            // --- CÁLCULO DE MÉTRICAS PRINCIPALES (Sin cambios) ---
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);
            var ventasHoyQuery = _context.Ventas.Where(v => v.FechaVenta >= hoy && v.FechaVenta < manana);

            viewModel.VentasHoy = await ventasHoyQuery.SumAsync(v => v.TotalPagar);
            viewModel.NumeroVentasHoy = await ventasHoyQuery.CountAsync();
            viewModel.TotalProductos = await _context.Productos.CountAsync();
            viewModel.TotalClientes = await _context.Clientes.CountAsync();

            // --- CÁLCULO DE DATOS PARA EL DASHBOARD (Ahora visible para todos) ---

            // 1. Productos con Stock Bajo (Top 5 más críticos)
            viewModel.ProductosBajoStock = await _context.Productos
                .Where(p => p.Estado == "Activo" && p.Stock <= p.StockMinimo && p.Stock > 0)
                .OrderBy(p => p.Stock)
                .Take(5)
                .ToListAsync();

            // 2. Productos Próximos a Vencer (en los próximos 30 días, Top 5 más cercanos)
            var fechaLimiteVencimiento = DateTime.Today.AddDays(30);
            viewModel.ProductosProximosAVencer = await _context.Productos
                .Where(p => p.Estado == "Activo" && p.FechaVencimiento != null && p.FechaVencimiento <= fechaLimiteVencimiento && p.FechaVencimiento >= DateTime.Today)
                .OrderBy(p => p.FechaVencimiento)
                .Take(5)
                .ToListAsync();

            // --- INICIO DE LA CORRECCIÓN DEL ERROR ---
            // 3. Top 5 Productos más vendidos (histórico) - CONSULTA CORREGIDA Y OPTIMIZADA
            viewModel.TopProductosVendidos = await _context.DetallesVenta
                .GroupBy(d => d.Producto.Nombre) // Agrupar directamente por el nombre del producto
                .Select(g => new {
                    NombreProducto = g.Key,
                    TotalVendido = g.Sum(d => d.Cantidad)
                })
                .OrderByDescending(r => r.TotalVendido)
                .Take(5)
                .ToDictionaryAsync(r => r.NombreProducto, r => r.TotalVendido);
            // --- FIN DE LA CORRECCIÓN DEL ERROR ---

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}