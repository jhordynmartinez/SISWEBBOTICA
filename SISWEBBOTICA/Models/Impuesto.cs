using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Impuesto
    {
        [Key]
        public int IdImpuesto { get; set; }

        [Required]
        [StringLength(10)]
        public string Abreviatura { get; set; }

        [Required]
        [StringLength(10)]
        public string PorcentajeTexto { get; set; }

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal ValorPorcentaje { get; set; }
    }
}