using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDBContext _context;

        public ProductoRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task CreateProductoAsync(Producto producto)
        {
            _context.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductoAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Producto> GetProductoByIdAsync(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<Producto> GetProductoConVentasByIdAsync(int id)
        {
            return await _context.Productos.Include(p => p.DetallesVenta).FirstOrDefaultAsync(p => p.IdProducto == id);
        }

        public async Task<List<Producto>> GetProductosAsync(string busqueda, string filtroStock)
        {
            var query = _context.Productos
                .Where(p => p.Estado == "Activo")
                .Include(p => p.Categoria)
                .Include(p => p.UnidadMedida)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda) ||
                                         p.CodigoBarras.Contains(busqueda) ||
                                         p.Categoria.Nombre.Contains(busqueda));
            }

            if (!string.IsNullOrEmpty(filtroStock))
            {
                switch (filtroStock)
                {
                    case "disponible": query = query.Where(p => p.Stock > 0); break;
                    case "bajo": query = query.Where(p => p.Stock > 0 && p.Stock <= 5); break;
                    case "sin": query = query.Where(p => p.Stock <= 0); break;
                }
            }

            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<bool> ProductoExistsAsync(int id)
        {
            return await _context.Productos.AnyAsync(e => e.IdProducto == id);
        }

        public async Task<bool> ProductoDuplicadoExistsAsync(string nombre, string presentacion, int idProducto = 0)
        {
            if (idProducto == 0) // Creando
            {
                return await _context.Productos.AnyAsync(p => p.Nombre == nombre && p.Presentacion == presentacion);
            }
            else // Editando
            {
                return await _context.Productos.AnyAsync(p => p.Nombre == nombre && p.Presentacion == presentacion && p.IdProducto != idProducto);
            }
        }

        public async Task UpdateProductoAsync(Producto producto)
        {
            _context.Update(producto);
            await _context.SaveChangesAsync();
        }
    }
}