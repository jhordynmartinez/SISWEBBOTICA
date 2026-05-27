using SISWEBBOTICA.Models.ML;

namespace SISWEBBOTICA.Services
{
    public interface IMLService
    {
        /// <summary>
        /// Entrena el modelo con datos históricos de ventas
        /// </summary>
        Task EntrenarModeloAsync();

        /// <summary>
        /// Predice la demanda para todos los productos
        /// </summary>
        Task<List<PrediccionDemandaResult>> PredecirDemandaAsync();

        /// <summary>
        /// Recomienda productos alternativos basado en principio activo, categoría o laboratorio
        /// </summary>
        Task<List<ProductoAlternativoResult>> RecomendarAlternativasAsync(int idProducto, string terminoBusqueda);

        /// <summary>
        /// Optimiza las compras sugeridas basándose en demanda y stock
        /// </summary>
        Task<List<CompraOptimaResult>> OptimizarComprasAsync();

        /// <summary>
        /// Obtiene los productos más vendidos en un período
        /// </summary>
        Task<List<ProductoRecomendacionML>> ObtenerProductosMasVendidosAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene productos de lenta rotación
        /// </summary>
        Task<List<ProductoRecomendacionML>> ObtenerProductosLentaRotacionAsync();

        /// <summary>
        /// Obtiene métricas del último entrenamiento (RMSE, R^2)
        /// </summary>
        Task<(double RMSE, double RSquared)> ObtenerMetricasAsync();

        /// <summary>
        /// Genera historial de ventas sintético para pruebas/entrenamiento
        /// </summary>
        Task GenerarHistorialVentasSinteticoAsync(int meses = 6);

        /// <summary>
        /// Genera un dataset sintético en memoria y entrena el modelo usando esos datos (no persiste en BD)
        /// </summary>
        Task EntrenarConHistorialSinteticoAsync(int meses = 6);
    }
}