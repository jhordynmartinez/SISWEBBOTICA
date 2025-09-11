using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class IngresoSalida
    {
        [Key]
        public int IdIngresoSalida { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        public DateTime Fecha { get; set; }

        public string? Concepto { get; set; }

        [StringLength(80)]
        public string? Operacion { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }
    }
}