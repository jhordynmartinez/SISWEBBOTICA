using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class RegistroVM
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [Display(Name = "Correo electrónico")]
        public string Login { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 8)]
        // --- INICIO DE LA CORRECCIÓN ---
        // Se comenta o elimina la Expresión Regular para que Identity se encargue de la validación.
        // [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "La contraseña debe tener al menos 8 caracteres e incluir letras y números.")]
        // --- FIN DE LA CORRECCIÓN ---
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Tipo de Usuario")]
        public string RolSeleccionado { get; set; }
    }
}