using Microsoft.ML.Data;

namespace SISWEBBOTICA.Models.ML
{
    /// <summary>
    /// Modelo para entrenar el algoritmo de predicción de demanda
    /// </summary>
    public class VentaHistorialML
    {
        // Características de entrada para el modelo
        public float CantidadVendida { get; set; }
        public float PrecioVenta { get; set; }
        public float DiaSemana { get; set; } // 1-7 (Lunes-Domingo)
        public float DiaMes { get; set; } // 1-31
        public float Mes { get; set; } // 1-12
        public float StockDisponible { get; set; }
        public float EsFinDeSemana { get; set; }
        public float EsInicioDeMes { get; set; } // Días 1-5
        
        // Etiqueta a predecir (cantidad futura)
        public float CantidadFutura { get; set; }
    }

    /// <summary>
    /// Resultado de la predicción de demanda
    /// </summary>
    public class PrediccionDemandaResult
    {
        [ColumnName("Score")]
        public float CantidadPredicha { get; set; }
        public float Confianza { get; set; } // 0-1
        public string NombreProducto { get; set; }
        public int IdProducto { get; set; }
        public float StockActual { get; set; }
        public float StockMinimo { get; set; }
        public string Recomendacion { get; set; } // "Comprar", "No Comprar", "Mantener"
        public int CantidadSugeridaCompra { get; set; }
    }

    /// <summary>
    /// Modelo para recomendación de productos alternativos
    /// </summary>
    public class ProductoRecomendacionML
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string PrincipioActivo { get; set; }
        public string Laboratorio { get; set; }
        public int IdCategoria { get; set; }
        public float PrecioMenor { get; set; }
        public float Stock { get; set; }
        public float VentasPromedio { get; set; }
    }

    /// <summary>
    /// Resultado de recomendación de producto alternativo
    /// </summary>
    public class ProductoAlternativoResult
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public string PrincipioActivo { get; set; }
        public string Laboratorio { get; set; }
        public float Precio { get; set; }
        public float Stock { get; set; }
        public float Similitud { get; set; } // 0-1 (qué tan similar es)
        public string MotivoRecomendacion { get; set; } // "Mismo principio activo", "Misma categoría", etc.
    }

    /// <summary>
    /// Optimización de compra sugerida
    /// </summary>
    public class CompraOptimaResult
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadSugerida { get; set; }
        public decimal CostoEstimado { get; set; }
        public decimal UtilidadEsperada { get; set; }
        public int DiasParaAgotarse { get; set; }
        public string Prioridad { get; set; } // "Alta", "Media", "Baja"
        public string Justificacion { get; set; }
    }
}