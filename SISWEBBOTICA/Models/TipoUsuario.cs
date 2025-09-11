using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class TipoUsuario
    {
        public TipoUsuario()
        {
            this.Usuarios = new HashSet<Usuario>();
        }

        [Key]
        public int IdTipoUsuario { get; set; }

        [Required]
        [StringLength(50)]
        public string Descripcion { get; set; }

        public virtual ICollection<Usuario> Usuarios { get; set; }
    }
}