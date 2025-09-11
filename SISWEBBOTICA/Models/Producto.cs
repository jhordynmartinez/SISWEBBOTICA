using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Producto
    {
        public Producto()
        {
            this.DetallesVenta = new HashSet<DetalleVenta>();
            this.DetallesCompra = new HashSet<DetalleCompra>();
            this.DetallesCotizacion = new HashSet<DetalleCotizacion>();
        }

        [Key]
        public int IdProducto { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        [Required]
        public int IdUnidadMedida { get; set; }

        [Required]
        [StringLength(255)]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required]
        [StringLength(60)]
        public string CodigoBarras { get; set; }

        [StringLength(255)]
        public string? Laboratorio { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecioCompra { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecioMenor { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrecioMayor { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Stock { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StockMinimo { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        [ForeignKey("IdCategoria")]
        public virtual Categoria Categoria { get; set; }

        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadMedida UnidadMedida { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; }
        public virtual ICollection<DetalleCompra> DetallesCompra { get; set; }
        public virtual ICollection<DetalleCotizacion> DetallesCotizacion { get; set; }
    }
}