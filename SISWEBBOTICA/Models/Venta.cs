using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Venta
    {
        public Venta()
        {
            this.DetallesVenta = new HashSet<DetalleVenta>();
            this.Pagos = new HashSet<Pago>();
        }

        [Key]
        public int IdVenta { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [Required]
        public int IdMoneda { get; set; }

        [StringLength(40)]
        public string NumeroComprobante { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPagar { get; set; }

        public DateTime FechaVenta { get; set; }

        [StringLength(50)]
        public string CondicionPago { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("IdMoneda")]
        public virtual Moneda Moneda { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; }
    }
}