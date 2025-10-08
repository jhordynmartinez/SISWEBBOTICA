using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.Models
{
    // Hereda de IdentityRole con una clave de tipo int
    public class TipoUsuario : IdentityRole<int>
    {
        public TipoUsuario() { }

        public TipoUsuario(string roleName) : base(roleName) { }

        // Puedes agregar propiedades personalizadas si las necesitas
        [StringLength(100)]
        public string? Descripcion { get; set; }
    }
}