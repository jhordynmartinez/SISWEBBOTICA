using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Gasto
    {
        [Key]
        public int IdGasto { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public int IdAperturaCaja { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaGasto { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IdAperturaCaja")]
        public virtual AperturaCaja AperturaCaja { get; set; }
    }
}