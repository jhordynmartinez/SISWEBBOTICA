using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public interface IProductoRepository
    {
        Task<List<Producto>> GetProductosAsync(string busqueda, string filtroStock);
        Task<Producto> GetProductoByIdAsync(int id);
        Task CreateProductoAsync(Producto producto);
        Task UpdateProductoAsync(Producto producto);
        Task DeleteProductoAsync(int id);
        Task<bool> ProductoExistsAsync(int id);
        Task<Producto> GetProductoConVentasByIdAsync(int id);
        Task<bool> ProductoDuplicadoExistsAsync(string nombre, string presentacion, int idProducto = 0);
    }
}