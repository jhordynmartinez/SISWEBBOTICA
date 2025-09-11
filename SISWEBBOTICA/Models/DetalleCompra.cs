using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class DetalleCompra
    {
        [Key]
        public int IdDetalleCompra { get; set; }

        [Required]
        public int IdCompra { get; set; }

        [Required]
        public int IdProducto { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Precio { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Importe { get; set; }

        [ForeignKey("IdCompra")]
        public virtual Compra Compra { get; set; }

        [ForeignKey("IdProducto")]
        public virtual Producto Producto { get; set; }
    }
}