using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Botica
    {
        public Botica()
        {
            this.Cotizaciones = new HashSet<Cotizacion>();
        }

        [Key]
        public int IdTienda { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [StringLength(11)]
        public string Ruc { get; set; }

        [StringLength(20)]
        public string? Celular { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string Direccion { get; set; }
        // --- INICIO DE LA MODIFICACIÓN ---

        [Required]
        [Display(Name = "Permitir Ventas con Stock Negativo")]
        public bool PermitirStockNegativo { get; set; } = false; // Por defecto, no se permite (H2)

        // --- FIN DE LA MODIFICACIÓN ---

        public virtual ICollection<Cotizacion> Cotizaciones { get; set; }
    }
}