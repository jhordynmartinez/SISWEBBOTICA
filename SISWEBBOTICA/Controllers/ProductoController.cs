using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly AppDBContext _context;

        public ProductoController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /Producto (CORREGIDO: Muestra solo productos activos)
        public async Task<IActionResult> Index()
        {
            var productos = _context.Productos
                                    .Where(p => p.Estado == "Activo")
                                    .Include(p => p.Categoria)
                                    .Include(p => p.UnidadMedida);
            return View(await productos.ToListAsync());
        }

        public IActionResult Create()
        {
            var viewModel = new ProductoVM
            {
                Producto = new Producto(),
                CategoriasList = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "IdCategoria", "Nombre"),
                UnidadesMedidaList = new SelectList(_context.UnidadesMedida.OrderBy(u => u.Nombre), "IdUnidadMedida", "Nombre")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoVM viewModel)
        {
            // El modelo Producto dentro del ViewModel se llena automáticamente
            if (ModelState.IsValid)
            {
                viewModel.Producto.Estado = "Activo"; // Aseguramos que el estado sea Activo al crear
                _context.Add(viewModel.Producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            viewModel.CategoriasList = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "IdCategoria", "Nombre", viewModel.Producto.IdCategoria);
            viewModel.UnidadesMedidaList = new SelectList(_context.UnidadesMedida.OrderBy(u => u.Nombre), "IdUnidadMedida", "Nombre", viewModel.Producto.IdUnidadMedida);
            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            var viewModel = new ProductoVM
            {
                Producto = producto,
                CategoriasList = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "IdCategoria", "Nombre", producto.IdCategoria),
                UnidadesMedidaList = new SelectList(_context.UnidadesMedida.OrderBy(u => u.Nombre), "IdUnidadMedida", "Nombre", producto.IdUnidadMedida)
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, ProductoVM viewModel)
        {
            if (id != viewModel.Producto.IdProducto) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Aseguramos que el estado no se cambie accidentalmente en este formulario
                    viewModel.Producto.Estado = "Activo";
                    _context.Update(viewModel.Producto);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(viewModel.Producto.IdProducto)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            viewModel.CategoriasList = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "IdCategoria", "Nombre", viewModel.Producto.IdCategoria);
            viewModel.UnidadesMedidaList = new SelectList(_context.UnidadesMedida.OrderBy(u => u.Nombre), "IdUnidadMedida", "Nombre", viewModel.Producto.IdUnidadMedida);
            return View(viewModel);
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.Include(p => p.Categoria).Include(p => p.UnidadMedida).FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        // POST: /Producto/Delete/5 (CORREGIDO: Implementa Eliminación Lógica)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.Estado = "Inactivo"; // Cambiamos el estado en lugar de borrar
                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto eliminado (marcado como inactivo) exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}