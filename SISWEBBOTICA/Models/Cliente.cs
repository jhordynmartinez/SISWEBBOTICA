using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Cliente
    {
        public Cliente()
        {
            this.Ventas = new HashSet<Venta>();
            this.Cotizaciones = new HashSet<Cotizacion>();
        }

        [Key]
        public int IdCliente { get; set; }

        [StringLength(20)]
        public string RucDni { get; set; }

        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(100)]
        public string? Apellido { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; }
    }
}