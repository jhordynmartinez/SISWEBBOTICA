using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Compra
    {
        public Compra()
        {
            this.DetallesCompra = new HashSet<DetalleCompra>();
        }

        [Key]
        public int IdCompra { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(6)]
        public string IdProveedor { get; set; }

        [Required]
        [StringLength(255)]
        public string NumeroComprobante { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCompra { get; set; }

        public DateTime FechaCompra { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdProveedor")]
        public virtual Proveedor Proveedor { get; set; }

        public virtual ICollection<DetalleCompra> DetallesCompra { get; set; }
    }
}