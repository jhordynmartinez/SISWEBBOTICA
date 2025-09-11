using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class Recordatorio
    {
        [Key]
        public int IdRecordatorio { get; set; }

        public string Descripcion { get; set; }

        public DateTime Fecha { get; set; }
    }
}