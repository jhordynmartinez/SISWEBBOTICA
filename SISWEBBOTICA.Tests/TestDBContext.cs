using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;

namespace SISWEBBOTICA.Tests
{
    // Este DbContext hereda del DbContext normal, no del de Identity
    public class TestDBContext : DbContext
    {
        public TestDBContext(DbContextOptions<TestDBContext> options) : base(options) { }

        // Incluimos todas las tablas que necesitamos para las pruebas
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
        // No necesitamos las tablas de Usuario/Identity aquí
    }
}