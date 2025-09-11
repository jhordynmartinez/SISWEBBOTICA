using System.Collections.Generic; // Asegúrate de tener este using
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Categoria
    {
        // 1. Añadir un constructor
        public Categoria()
        {
            // 2. Inicializar la colección aquí dentro
            this.Productos = new HashSet<Producto>();
        }

        [Key]
        public int IdCategoria { get; set; }

        [Required]
        [StringLength(60)]
        public string Nombre { get; set; }

        // 3. Dejar la propiedad de navegación sin el inicializador (= new List...)
        public virtual ICollection<Producto> Productos { get; set; }
    }
}