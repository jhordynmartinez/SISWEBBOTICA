using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdVenta { get; set; }

        [Required]
        public int IdMetodoPago { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaPago { get; set; }

        [StringLength(100)]
        public string? Referencia { get; set; }

        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; }

        [ForeignKey("IdMetodoPago")]
        public virtual MetodoPago MetodoPago { get; set; }
    }
}