using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class MetodoPago
    {
        public MetodoPago()
        {
            this.Pagos = new HashSet<Pago>();
        }

        [Key]
        public int IdMetodoPago { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public bool RequiereReferencia { get; set; }

        public virtual ICollection<Pago> Pagos { get; set; }
    }
}