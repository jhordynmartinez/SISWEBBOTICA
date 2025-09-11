using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class DetalleCotizacion
    {
        [Key]
        public int IdDetalleCotizacion { get; set; }

        [Required]
        public int IdCotizacion { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Importe { get; set; }

        [ForeignKey("IdCotizacion")]
        public virtual Cotizacion Cotizacion { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }
    }
}