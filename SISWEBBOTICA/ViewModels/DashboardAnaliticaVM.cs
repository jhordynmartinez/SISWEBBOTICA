using SISWEBBOTICA.Models.ML;

namespace SISWEBBOTICA.ViewModels
{
    public class DashboardAnaliticaVM
    {
        // KPIs principales
        public decimal VentaTotalHoy { get; set; }
        public decimal VentaTotalMes { get; set; }
        public int ProductosVendidosHoy { get; set; }
        public int TicketsPromedio { get; set; }
        
        // Predicciones
        public List<PrediccionDemandaResult> Predicciones { get; set; } = new();
        
        // Productos más vendidos
        public List<ProductoRecomendacionML> ProductosMasVendidos { get; set; } = new();
        
        // Productos lenta rotación
        public List<ProductoRecomendacionML> ProductosLentaRotacion { get; set; } = new();
        
        // Alertas de stock
        public List<PrediccionDemandaResult> AlertasStock { get; set; } = new();

        // Métricas del último entrenamiento
        public double MetricRMSE { get; set; }
        public double MetricRSquared { get; set; }
    }
}