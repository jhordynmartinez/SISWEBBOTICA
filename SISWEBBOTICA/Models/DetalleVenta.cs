using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class DetalleVenta
    {
        [Key]
        public int IdDetalleVenta { get; set; }

        [Required]
        public int IdVenta { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Importe { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Utilidad { get; set; }

        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }
    }
}