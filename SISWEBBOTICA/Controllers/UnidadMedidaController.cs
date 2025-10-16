using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize]
    public class UnidadMedidaController : Controller
    {
        private readonly AppDBContext _context;

        public UnidadMedidaController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /UnidadMedida
        public async Task<IActionResult> Index()
        {
            return View(await _context.UnidadesMedida.ToListAsync());
        }

        // GET: /UnidadMedida/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /UnidadMedida/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Simbolo")] UnidadMedida unidadMedida)
        {
            if (ModelState.IsValid)
            {
                _context.Add(unidadMedida);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Unidad de Medida creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(unidadMedida);
        }

        // GET: /UnidadMedida/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadMedida = await _context.UnidadesMedida.FindAsync(id);
            if (unidadMedida == null)
            {
                return NotFound();
            }
            return View(unidadMedida);
        }

        // POST: /UnidadMedida/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdUnidadMedida,Nombre,Simbolo")] UnidadMedida unidadMedida)
        {
            if (id != unidadMedida.IdUnidadMedida)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unidadMedida);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Unidad de Medida actualizada exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnidadMedidaExists(unidadMedida.IdUnidadMedida))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(unidadMedida);
        }

        // GET: /UnidadMedida/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidadMedida = await _context.UnidadesMedida
                .FirstOrDefaultAsync(m => m.IdUnidadMedida == id);
            if (unidadMedida == null)
            {
                return NotFound();
            }

            return View(unidadMedida);
        }

        // POST: /UnidadMedida/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unidadMedida = await _context.UnidadesMedida.FindAsync(id);
            if (unidadMedida != null)
            {
                _context.UnidadesMedida.Remove(unidadMedida);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Unidad de Medida eliminada exitosamente.";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool UnidadMedidaExists(int id)
        {
            return _context.UnidadesMedida.Any(e => e.IdUnidadMedida == id);
        }
    }
}