using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;

namespace SISWEBBOTICA.Data
{
    // Hereda de IdentityDbContext y especifica los tipos personalizados
    public class AppDBContext : IdentityDbContext<Usuario, TipoUsuario, int,
        IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        // --- DbSet para las tablas que NO son de Identity ---
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizacion { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<AperturaCaja> AperturasCaja { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<IngresoSalida> IngresosSalidas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<Impuesto> Impuestos { get; set; }
        public DbSet<Botica> Boticas { get; set; }
        public DbSet<ConfiguracionNumeracion> ConfiguracionesNumeracion { get; set; }
        public DbSet<Nota> Notas { get; set; }
        public DbSet<Recordatorio> Recordatorios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // MUY IMPORTANTE: Esta línea configura las tablas de Identity

            // Renombrar tablas de Identity (Opcional, pero recomendado para mantener consistencia)
            modelBuilder.Entity<Usuario>(entity => { entity.ToTable(name: "Usuarios"); });
            modelBuilder.Entity<TipoUsuario>(entity => { entity.ToTable(name: "Roles"); });
            modelBuilder.Entity<IdentityUserRole<int>>(entity => { entity.ToTable("UsuarioRoles"); });
            modelBuilder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable("UsuarioClaims"); });
            modelBuilder.Entity<IdentityUserLogin<int>>(entity => { entity.ToTable("UsuarioLogins"); });
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable("RolClaims"); });
            modelBuilder.Entity<IdentityUserToken<int>>(entity => { entity.ToTable("UsuarioTokens"); });

            // Configuraciones de relaciones personalizadas (las que ya tenías)
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            // ... (mantén las otras configuraciones de OnModelCreating que ya tenías)
        }
    }
}