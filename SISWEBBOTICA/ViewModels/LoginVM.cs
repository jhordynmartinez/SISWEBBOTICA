using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "El correo o nombre de usuario es obligatorio.")]
        [Display(Name = "Correo o Usuario")]
        public string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }
    }
}