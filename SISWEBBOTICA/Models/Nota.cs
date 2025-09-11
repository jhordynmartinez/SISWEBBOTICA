using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Nota
    {
        [Key]
        public int IdNota { get; set; }

        [Required]
        public int IdUsuario { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public DateTime FechaHora { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario Usuario { get; set; }
    }
}