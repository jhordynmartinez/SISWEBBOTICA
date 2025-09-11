using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;

namespace SISWEBBOTICA.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        // --- DbSet para cada una de las tablas de la base de datos ---

        // Entidades Principales
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }

        // Entidades Transaccionales
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }

        // Entidades de Pago
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }

        // Entidades Operacionales y Financieras
        public DbSet<AperturaCaja> AperturasCaja { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<IngresoSalida> IngresosSalidas { get; set; }

        // Entidades de Catálogo y Configuración
        public DbSet<TipoUsuario> TiposUsuario { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<Impuesto> Impuestos { get; set; }
        public DbSet<Botica> Boticas { get; set; } // O Tiendas si le pusiste ese nombre
        public DbSet<ConfiguracionNumeracion> ConfiguracionesNumeracion { get; set; }

        // Entidades de Utilidad
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones Adicionales (Fluent API)
            // Aunque las Data Annotations ya hacen la mayor parte del trabajo, aquí
            // se pueden definir relaciones o restricciones más complejas si fuera necesario.

            // --- Ejemplo de configuración para DetalleCompra (si no tuviera su propia PK) ---
            // modelBuilder.Entity<DetalleCompra>()
            //     .HasKey(dc => new { dc.IdCompra, dc.IdProducto }); // Clave primaria compuesta

            // --- Relaciones (EF Core las infiere por las propiedades de navegación, pero aquí se pueden hacer explícitas) ---

            // Relación Venta -> DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra una venta, se borran sus detalles

            // Relación Producto -> DetalleVenta
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict); // Evita borrar un producto si está en una venta

            // Relación Compra -> DetalleCompra
            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Compra)
                .WithMany(c => c.DetallesCompra)
                .HasForeignKey(d => d.IdCompra)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Producto -> DetalleCompra
            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Venta -> Pago
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Venta)
                .WithMany(v => v.Pagos)
                .HasForeignKey(p => p.IdVenta)
                .OnDelete(DeleteBehavior.Cascade); // Si se borra una venta, se borran sus pagos

            // Se puede hacer lo mismo para las demás relaciones para tener un control explícito
            // sobre el comportamiento de la base de datos (como la eliminación en cascada).
            // Por defecto, EF Core infiere las relaciones y establece políticas de eliminación razonables.
        }
    }
}
