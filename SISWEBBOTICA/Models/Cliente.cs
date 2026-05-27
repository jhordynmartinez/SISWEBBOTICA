using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Cliente
    {
        public Cliente()
        {
            this.Ventas = new HashSet<Venta>();
            this.Cotizaciones = new HashSet<Cotizacion>();
            this.FechaRegistro = DateTime.Now;
            this.Estado = "Activo";
        }

        [Key]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El RUC/DNI es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "RUC/DNI")]
        public string RucDni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string? Apellido { get; set; }

        [StringLength(255)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Activo";

        [Required]
        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Es Cliente VIP")]
        public bool EsClienteVIP { get; set; } = false;

        [Display(Name = "Notas")]
        public string? Notas { get; set; }

        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; }
    }
}