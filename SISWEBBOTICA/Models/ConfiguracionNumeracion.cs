using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SISWEBBOTICA.Models
{
    public class ConfiguracionNumeracion
    {
        [Key]
        public int IdConfigNumeracion { get; set; }

        [StringLength(10)]
        public string? SerieNotaVenta { get; set; }
        public int? NumNotaVenta { get; set; }

        [StringLength(10)]
        public string? SerieBoleta { get; set; }
        [StringLength(20)]
        public string? NumBoleta { get; set; }

        [StringLength(10)]
        public string? SerieFactura { get; set; }
        [StringLength(20)]
        public string? NumFactura { get; set; }
    }
}