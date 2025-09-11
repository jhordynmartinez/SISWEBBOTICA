using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class UnidadMedida
    {
        public UnidadMedida()
        {
            this.Productos = new HashSet<Producto>();
        }

        [Key]
        public int IdUnidadMedida { get; set; }

        [Required]
        [StringLength(55)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(20)]
        public string Simbolo { get; set; }

        public virtual ICollection<Producto> Productos { get; set; }
    }
}