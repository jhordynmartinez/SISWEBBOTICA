using SISWEBBOTICA.Models.ML;

namespace SISWEBBOTICA.ViewModels
{
    public class RecomendacionVM
    {
        // Para búsqueda de producto
        public string TerminoBusqueda { get; set; }
        public int? IdProductoSeleccionado { get; set; }
        
        // Producto original buscado
        public string ProductoOriginalNombre { get; set; }
        public bool ProductoOriginalSinStock { get; set; }
        
        // Alternativas recomendadas
        public List<ProductoAlternativoResult> Alternativas { get; set; } = new();
        
        // Para optimización de compras
        public List<CompraOptimaResult> ComprasSugeridas { get; set; } = new();
        
        // Filtros
        public string FiltroPrioridad { get; set; } // "Todas", "Alta", "Media", "Baja"
    }
}