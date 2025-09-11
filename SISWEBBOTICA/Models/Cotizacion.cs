using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Cotizacion
    {
        public Cotizacion()
        {
            this.DetallesCotizacion = new HashSet<DetalleCotizacion>();
        }

        [Key]
        public int IdCotizacion { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [Required]
        public int IdTienda { get; set; }

        [Required]
        public int IdMoneda { get; set; }

        [StringLength(255)]
        public string NumeroCotizacion { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPagar { get; set; }

        [StringLength(255)]
        public string PlazoEntrega { get; set; }

        [StringLength(255)]
        public string Validez { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("IdTienda")]
        public virtual Botica Botica { get; set; }

        [ForeignKey("IdMoneda")]
        public virtual Moneda Moneda { get; set; }

        public virtual ICollection<DetalleCotizacion> DetallesCotizacion { get; set; }
    }
}