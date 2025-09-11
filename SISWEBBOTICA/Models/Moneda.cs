using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Moneda
    {
        public Moneda()
        {
            this.Ventas = new HashSet<Venta>();
            this.Cotizaciones = new HashSet<Cotizacion>();
        }

        [Key]
        public int IdMoneda { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(10)]
        public string Simbolo { get; set; }

        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; }
    }
}