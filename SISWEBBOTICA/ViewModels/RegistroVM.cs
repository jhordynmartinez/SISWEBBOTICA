using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class RegistroVM
    {

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string? Apellido { get; set; }

        [Required, StringLength(30)]
        public string Login { get; set; }

        [Required, DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [Required, Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasena { get; set; }

        [Required]
        public int IdTipoUsuario { get; set; } // Admin o Vendedor
    }
}
