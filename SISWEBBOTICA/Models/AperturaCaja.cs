using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class AperturaCaja
    {
        public AperturaCaja()
        {
            this.Gastos = new HashSet<Gasto>();
        }

        [Key]
        public int IdAperturaCaja { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime FechaApertura { get; set; }

        public virtual ICollection<Gasto> Gastos { get; set; }
    }
}