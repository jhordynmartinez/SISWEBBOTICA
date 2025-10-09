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

        // --- MÉTODOS GET (SIN CAMBIOS, ESTÁN CORRECTOS) ---

        public async Task<IActionResult> Index()
        {
            var productos = _context.Productos.Include(p => p.Categoria).Include(p => p.UnidadMedida);
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

        // --- INICIO DE LA CORRECCIÓN EN MÉTODOS POST ---

        // POST: /Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "Producto")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            // Si la validación falla, debemos recrear el ViewModel completo
            var viewModel = new ProductoVM
            {
                Producto = producto, // Pasamos el producto con los datos que el usuario ya ingresó
                CategoriasList = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "IdCategoria", "Nombre", producto.IdCategoria),
                UnidadesMedidaList = new SelectList(_context.UnidadesMedida.OrderBy(u => u.Nombre), "IdUnidadMedida", "Nombre", producto.IdUnidadMedida)
            };
            return View(viewModel);
        }

        // POST: /Producto/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Producto")] Producto producto)
        {
            if (id != producto.IdProducto) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.IdProducto)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Si la validación falla, recreamos el ViewModel
            var viewModel = new ProductoVM
            {
                Producto = producto,
                CategoriasList = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "IdCategoria", "Nombre", producto.IdCategoria),
                UnidadesMedidaList = new SelectList(_context.UnidadesMedida.OrderBy(u => u.Nombre), "IdUnidadMedida", "Nombre", producto.IdUnidadMedida)
            };
            return View(viewModel);
        }

        // --- FIN DE LA CORRECCIÓN ---

        // --- MÉTODOS DELETE (SIN CAMBIOS, ESTÁN CORRECTOS) ---
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var producto = await _context.Productos.Include(p => p.Categoria).Include(p => p.UnidadMedida).FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto eliminado exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}