using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Proveedor
    {
        public Proveedor()
        {
            this.Compras = new HashSet<Compra>();
        }

        [Key]
        [StringLength(6)]
        public string IdProveedor { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [StringLength(11)]
        public string? Ruc { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public virtual ICollection<Compra> Compras { get; set; }
    }
}