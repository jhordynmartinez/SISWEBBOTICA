using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    // Hereda de IdentityUser con una clave de tipo int
    public class Usuario : IdentityUser<int>
    {
        public Usuario()
        {
            this.Ventas = new HashSet<Venta>();
            this.Compras = new HashSet<Compra>();
            this.Gastos = new HashSet<Gasto>();
            this.Cotizaciones = new HashSet<Cotizacion>();
            this.Notas = new HashSet<Nota>();
            this.IngresosSalidas = new HashSet<IngresoSalida>();
        }

        // Propiedades personalizadas que no están en IdentityUser
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string? Apellido { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        // Propiedades de Navegación
        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<Compra> Compras { get; set; }
        public virtual ICollection<Gasto> Gastos { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; }
        public virtual ICollection<Nota> Notas { get; set; }
        public virtual ICollection<IngresoSalida> IngresosSalidas { get; set; }
    }
}