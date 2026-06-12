using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class ClienteVM
    {
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
        [Display(Name = "Direcci�n")]
        public string? Direccion { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "El tel�fono debe ser v�lido.")]
        [Display(Name = "Tel�fono")]
        public string? Telefono { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "El correo debe ser v�lido.")]
        [Display(Name = "Correo Electr�nico")]
        public string? Email { get; set; }
    }

    public class ClienteCreateVM
    {
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
        [Display(Name = "Direcci�n")]
        public string? Direccion { get; set; }

        [StringLength(20)]
        [Display(Name = "Tel�fono")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es v�lido.")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Es Cliente VIP")]
        public bool EsClienteVIP { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }

    public class ClienteEditVM : ClienteCreateVM
    {
        public int IdCliente { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; }
    }

    public class ClienteListVM
    {
        public int IdCliente { get; set; }
        public string RucDni { get; set; }
        public string Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public bool EsClienteVIP { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int CantidadCompras { get; set; }
    }

    public class ClienteSearchVM
    {
        public List<ClienteListVM> Clientes { get; set; } = new List<ClienteListVM>();
        public string TerminoBusqueda { get; set; }
        public int TotalRegistros { get; set; }
    }
}
