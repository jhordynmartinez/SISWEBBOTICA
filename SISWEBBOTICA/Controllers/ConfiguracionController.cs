using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize(Roles = "Administrador")] // Solo el Administrador puede acceder
    public class ConfiguracionController : Controller
    {
        private readonly AppDBContext _context;

        public ConfiguracionController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /Configuracion
        public async Task<IActionResult> Index()
        {
            var tienda = await _context.Boticas.FirstOrDefaultAsync();
            if (tienda == null)
            {
                // Si no hay tienda, creamos una por defecto
                tienda = new Botica { Nombre = "Mi Botica", Ruc = "12345678901", Direccion = "Mi Dirección", PermitirStockNegativo = false };
                _context.Boticas.Add(tienda);
                await _context.SaveChangesAsync();
            }
            return View(tienda);
        }

        // POST: /Configuracion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Botica boticaModel)
        {
            if (ModelState.IsValid)
            {
                var tiendaEnDB = await _context.Boticas.FindAsync(boticaModel.IdTienda);
                if (tiendaEnDB != null)
                {
                    // Actualizamos solo los campos que permitimos cambiar en el formulario
                    tiendaEnDB.Nombre = boticaModel.Nombre;
                    tiendaEnDB.Ruc = boticaModel.Ruc;
                    tiendaEnDB.Direccion = boticaModel.Direccion;
                    tiendaEnDB.PermitirStockNegativo = boticaModel.PermitirStockNegativo;

                    _context.Update(tiendaEnDB);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Configuración guardada exitosamente.";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(boticaModel);
        }
    }
}