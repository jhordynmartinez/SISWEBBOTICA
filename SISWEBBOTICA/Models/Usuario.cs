using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Usuario
    {
        public Usuario()
        {
            this.Ventas = new HashSet<Venta>();
            this.Compras = new HashSet<Compra>();
            this.Gastos = new HashSet<Gasto>();
            this.Cotizaciones = new HashSet<Cotizacion>();
            this.Notas = new HashSet<Nota>();
            this.IngresosSalidas = new HashSet<IngresoSalida>();
        }

        [Key]
        public int IdUsuario { get; set; }

        [Required]
        public int IdTipoUsuario { get; set; }

        [StringLength(12)]
        public string? Dni { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string? Apellido { get; set; }

        [StringLength(20)]
        public string? Celular { get; set; }

        [StringLength(255)]
        public string? Direccion { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; }

        [Required]
        [StringLength(30)]
        public string Login { get; set; }

        [Required]
        [StringLength(255)]
        public string Contrasena { get; set; }

        [StringLength(255)]
        public string? PreguntaSeguridad { get; set; }

        [StringLength(255)]
        public string? RespuestaSeguridad { get; set; }

        public DateTime FechaRegistro { get; set; }

        [ForeignKey("IdTipoUsuario")]
        public virtual TipoUsuario TipoUsuario { get; set; }
        public virtual ICollection<Venta> Ventas { get; set; }
        public virtual ICollection<Compra> Compras { get; set; }
        public virtual ICollection<Gasto> Gastos { get; set; }
        public virtual ICollection<Cotizacion> Cotizaciones { get; set; }
        public virtual ICollection<Nota> Notas { get; set; }
        public virtual ICollection<IngresoSalida> IngresosSalidas { get; set; }
    }
}