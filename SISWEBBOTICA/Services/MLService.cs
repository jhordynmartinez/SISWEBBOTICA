using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.Models.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public class MLService : IMLService
    {
        private readonly AppDBContext _context;
        private readonly MLContext _mlContext;
        private ITransformer _modeloPrediccion;
        private PredictionEngine<VentaHistorialML, PrediccionDemandaResult> _predictionEngine;
        private RegressionMetrics _metrics;

        public MLService(AppDBContext context)
        {
            _context = context;
            _mlContext = new MLContext(seed: 0);
        }

        /// <summary>
        /// Genera un dataset sintético en memoria y entrena el modelo usando esos datos (no persiste en BD)
        /// </summary>
        public async Task EntrenarConHistorialSinteticoAsync(int meses = 6)
        {
            // Generar datos sintéticos en memoria
            var productos = await _context.Productos.Where(p => p.Estado == "Activo").ToListAsync();
            if (!productos.Any()) return;

            var rnd = new Random(0);
            var fechaInicio = DateTime.Today.AddMonths(-meses);
            var hoy = DateTime.Today;

            var datos = new List<VentaHistorialML>();

            foreach (var producto in productos)
            {
                // base de venta por producto
                // Calcular media base por producto usando StockMinimo (usar decimal->double conversion)
                double baseVal = producto.StockMinimo > 0 ? (double)(producto.StockMinimo / 5m) : 1.0;
                float baseMean = (float)Math.Max(0.5, Math.Min(5, baseVal));

                // generar series diarias
                List<float> serie = new List<float>();
                for (var dia = fechaInicio; dia <= hoy; dia = dia.AddDays(1))
                {
                    // variación día a día
                    var weekend = (dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday) ? 1.3f : 1.0f;
                    var noise = 1.0 + ((float)rnd.NextDouble() - 0.5f) * 0.6f; // +/-30%
                    var val = Math.Max(0f, baseMean * weekend * noise);
                    serie.Add((float)Math.Round(val));
                }

                // crear ejemplos con label = siguiente día
                for (int i = 0; i < serie.Count - 1; i++)
                {
                    var dia = fechaInicio.AddDays(i);
                    datos.Add(new VentaHistorialML
                    {
                        CantidadVendida = serie[i],
                        PrecioVenta = (float)producto.PrecioMenor,
                        DiaSemana = (float)((int)dia.DayOfWeek + 1),
                        DiaMes = (float)dia.Day,
                        Mes = (float)dia.Month,
                        StockDisponible = (float)producto.Stock,
                        EsFinDeSemana = dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday ? 1f : 0f,
                        EsInicioDeMes = dia.Day <= 5 ? 1f : 0f,
                        CantidadFutura = serie[i + 1]
                    });
                }
            }

            if (!datos.Any()) return;

            var dataView = _mlContext.Data.LoadFromEnumerable(datos);

            var pipeline = _mlContext.Transforms.CopyColumns("Label", nameof(VentaHistorialML.CantidadFutura))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(VentaHistorialML.CantidadVendida),
                    nameof(VentaHistorialML.PrecioVenta),
                    nameof(VentaHistorialML.DiaSemana),
                    nameof(VentaHistorialML.DiaMes),
                    nameof(VentaHistorialML.Mes),
                    nameof(VentaHistorialML.StockDisponible),
                    nameof(VentaHistorialML.EsFinDeSemana),
                    nameof(VentaHistorialML.EsInicioDeMes)))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            _modeloPrediccion = pipeline.Fit(split.TrainSet);
            var predictions = _modeloPrediccion.Transform(split.TestSet);
            _metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<VentaHistorialML, PrediccionDemandaResult>(_modeloPrediccion);
        }

        /// <summary>
        /// Entrena el modelo con datos históricos de ventas
        /// </summary>
        public async Task EntrenarModeloAsync()
        {
            // Obtener datos históricos de ventas (últimos 6 meses)
            var fechaInicio = DateTime.Now.AddMonths(-6);
            var detallesVenta = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta.FechaVenta >= fechaInicio)
                .ToListAsync();

            if (!detallesVenta.Any())
            {
                return; // No hay datos suficientes para entrenar
            }

            // Preparar datos para entrenamiento
            var datosEntrenamiento = new List<VentaHistorialML>();
            foreach (var detalle in detallesVenta)
            {
                var venta = detalle.Venta;
                datosEntrenamiento.Add(new VentaHistorialML
                {
                    CantidadVendida = (float)detalle.Cantidad,
                    PrecioVenta = (float)detalle.Precio,
                    DiaSemana = (float)((int)venta.FechaVenta.DayOfWeek + 1),
                    DiaMes = (float)venta.FechaVenta.Day,
                    Mes = (float)venta.FechaVenta.Month,
                    StockDisponible = (float)detalle.Producto.Stock,
                    EsFinDeSemana = venta.FechaVenta.DayOfWeek == DayOfWeek.Saturday || venta.FechaVenta.DayOfWeek == DayOfWeek.Sunday ? 1f : 0f,
                    EsInicioDeMes = venta.FechaVenta.Day <= 5 ? 1f : 0f,
                    CantidadFutura = (float)detalle.Cantidad // Label para entrenamiento
                });
            }

            var dataView = _mlContext.Data.LoadFromEnumerable(datosEntrenamiento);

            // Pipeline de transformación y entrenamiento (regresión)
            var pipeline = _mlContext.Transforms.CopyColumns("Label", nameof(VentaHistorialML.CantidadFutura))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(VentaHistorialML.CantidadVendida),
                    nameof(VentaHistorialML.PrecioVenta),
                    nameof(VentaHistorialML.DiaSemana),
                    nameof(VentaHistorialML.DiaMes),
                    nameof(VentaHistorialML.Mes),
                    nameof(VentaHistorialML.StockDisponible),
                    nameof(VentaHistorialML.EsFinDeSemana),
                    nameof(VentaHistorialML.EsInicioDeMes)))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features"));

            // Separar en entrenamiento/test para obtener métricas
            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Entrenar modelo con conjunto de entrenamiento
            _modeloPrediccion = pipeline.Fit(split.TrainSet);

            // Evaluar en conjunto de prueba
            var predictions = _modeloPrediccion.Transform(split.TestSet);
            _metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            // Crear motor de predicción para inferencia en tiempo real
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<VentaHistorialML, PrediccionDemandaResult>(_modeloPrediccion);
        }

        /// <summary>
        /// Predice la demanda para todos los productos
        /// </summary>
        public async Task<List<PrediccionDemandaResult>> PredecirDemandaAsync()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Estado == "Activo")
                .ToListAsync();

            var resultados = new List<PrediccionDemandaResult>();

            // Obtener ventas de los últimos 30 días para calcular promedio
            var fechaInicio = DateTime.Now.AddDays(-30);
            var ventasRecientes = await _context.DetallesVenta
                .Include(d => d.Venta)
                .Where(d => d.Venta.FechaVenta >= fechaInicio)
                .ToListAsync();

            foreach (var producto in productos)
            {
                var ventasProducto = ventasRecientes
                    .Where(d => d.IdProducto == producto.IdProducto)
                    .Sum(d => d.Cantidad);

                var promedioVentasDiarias = ventasProducto / 30.0m;
                var diasParaAgotarse = producto.Stock > 0 ? (int)(producto.Stock / (promedioVentasDiarias > 0 ? promedioVentasDiarias : 1)) : 0;

                string recomendacion;
                int cantidadSugerida = 0;

                if (producto.Stock <= producto.StockMinimo)
                {
                    recomendacion = "Comprar Urgente";
                    cantidadSugerida = (int)(producto.StockMinimo * 2 - producto.Stock);
                }
                else if (diasParaAgotarse <= 7)
                {
                    recomendacion = "Comprar";
                    cantidadSugerida = (int)(producto.StockMinimo * 3 - producto.Stock);
                }
                else if (diasParaAgotarse <= 30)
                {
                    recomendacion = "Mantener";
                }
                else
                {
                    recomendacion = "No Comprar";
                }

                resultados.Add(new PrediccionDemandaResult
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    StockActual = (float)producto.Stock,
                    StockMinimo = (float)producto.StockMinimo,
                    CantidadPredicha = (float)(promedioVentasDiarias * 7m), // Predicción para próxima semana
                    Confianza = 0.85f, // Confianza base (se puede mejorar con más datos)
                    Recomendacion = recomendacion,
                    CantidadSugeridaCompra = cantidadSugerida
                });
            }

            return resultados.OrderByDescending(r => r.StockActual <= r.StockMinimo).ToList();
        }

        /// <summary>
        /// Devuelve métricas del último entrenamiento
        /// </summary>
        public Task<(double RMSE, double RSquared)> ObtenerMetricasAsync()
        {
            if (_metrics == null) return Task.FromResult((0.0, 0.0));
            return Task.FromResult((_metrics.RootMeanSquaredError, _metrics.RSquared));
        }

        /// <summary>
        /// Genera ventas sintéticas en la base de datos para pruebas y entrenamiento
        /// </summary>
        public async Task GenerarHistorialVentasSinteticoAsync(int meses = 6)
        {
            var productos = await _context.Productos.Where(p => p.Estado == "Activo").ToListAsync();
            if (!productos.Any()) return;

            var cliente = await _context.Clientes.FirstOrDefaultAsync() ?? new Cliente { Nombre = "PÚBLICO GENERAL", RucDni = "00000000" };
            if (cliente.IdCliente == 0)
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            var moneda = await _context.Monedas.FirstOrDefaultAsync();
            int idMoneda = moneda?.IdMoneda ?? 1;

            var rnd = new Random(0);
            var fechaInicio = DateTime.Today.AddMonths(-meses);
            var hoy = DateTime.Today;

            for (var dia = fechaInicio; dia <= hoy; dia = dia.AddDays(1))
            {
                foreach (var producto in productos)
                {
                    // Probabilidad de venta por día según stock y ventas previas
                    var probVenta = Math.Min(0.5, 0.1 + (producto.StockMinimo > 0 ? 0.1 : 0));
                    if (rnd.NextDouble() > probVenta) continue;

                    int cantidad = rnd.Next(1, 4); // 1..3 unidades

                    var venta = new Venta
                    {
                        IdUsuario = 1,
                        IdCliente = cliente.IdCliente,
                        IdMoneda = idMoneda,
                        NumeroComprobante = $"SYN-{Guid.NewGuid().ToString().Substring(0, 8)}",
                        TotalPagar = producto.PrecioMenor * cantidad,
                        FechaVenta = dia,
                        CondicionPago = "Contado"
                    };

                    _context.Ventas.Add(venta);
                    await _context.SaveChangesAsync(); // Necesario para obtener IdVenta

                    var detalle = new DetalleVenta
                    {
                        IdVenta = venta.IdVenta,
                        IdProducto = producto.IdProducto,
                        Precio = producto.PrecioMenor,
                        Cantidad = cantidad,
                        Importe = producto.PrecioMenor * cantidad,
                        Utilidad = (producto.PrecioMenor - producto.PrecioCompra) * cantidad
                    };

                    _context.DetallesVenta.Add(detalle);
                    // No actualizar stock real; esto es solo para generar historial
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Recomienda productos alternativos basado en principio activo, categoría o laboratorio
        /// </summary>
        public async Task<List<ProductoAlternativoResult>> RecomendarAlternativasAsync(int idProducto, string terminoBusqueda)
        {
            var productoOriginal = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto);

            if (productoOriginal == null)
            {
                return new List<ProductoAlternativoResult>();
            }

            // Buscar productos alternativos con stock
            var alternativas = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.IdProducto != idProducto && p.Estado == "Activo" && p.Stock > 0)
                .ToListAsync();

            var resultados = new List<ProductoAlternativoResult>();

            foreach (var alt in alternativas)
            {
                float similitud = 0;
                string motivo = "";

                // Mismo principio activo (máxima prioridad)
                if (!string.IsNullOrEmpty(alt.PrincipioActivo) && 
                    !string.IsNullOrEmpty(productoOriginal.PrincipioActivo) &&
                    alt.PrincipioActivo.Contains(productoOriginal.PrincipioActivo, StringComparison.OrdinalIgnoreCase))
                {
                    similitud = 0.95f;
                    motivo = "Mismo principio activo";
                }
                // Mismo laboratorio
                else if (!string.IsNullOrEmpty(alt.Laboratorio) && 
                         !string.IsNullOrEmpty(productoOriginal.Laboratorio) &&
                         alt.Laboratorio == productoOriginal.Laboratorio)
                {
                    similitud = 0.75f;
                    motivo = "Mismo laboratorio";
                }
                // Misma categoría
                else if (alt.IdCategoria == productoOriginal.IdCategoria)
                {
                    similitud = 0.60f;
                    motivo = "Misma categoría terapéutica";
                }
                // Búsqueda por término
                else if (!string.IsNullOrEmpty(terminoBusqueda) && 
                         alt.Nombre.Contains(terminoBusqueda, StringComparison.OrdinalIgnoreCase))
                {
                    similitud = 0.50f;
                    motivo = "Nombre similar";
                }

                if (similitud > 0.4f) // Solo mostrar alternativas relevantes
                {
                    resultados.Add(new ProductoAlternativoResult
                    {
                        IdProducto = alt.IdProducto,
                        Nombre = alt.Nombre,
                        PrincipioActivo = alt.PrincipioActivo ?? "N/A",
                        Laboratorio = alt.Laboratorio ?? "N/A",
                        Precio = (float)alt.PrecioMenor,
                        Stock = (float)alt.Stock,
                        Similitud = similitud,
                        MotivoRecomendacion = motivo
                    });
                }
            }

            return resultados.OrderByDescending(r => r.Similitud).Take(5).ToList();
        }

        /// <summary>
        /// Optimiza las compras sugeridas basándose en demanda y stock
        /// </summary>
        public async Task<List<CompraOptimaResult>> OptimizarComprasAsync()
        {
            var predicciones = await PredecirDemandaAsync();
            var resultados = new List<CompraOptimaResult>();

            foreach (var prediccion in predicciones.Where(p => p.Recomendacion != "No Comprar"))
            {
                var producto = await _context.Productos.FindAsync(prediccion.IdProducto);
                if (producto == null) continue;

                var diasParaAgotarse = producto.Stock > 0
                    ? (int)((float)producto.Stock / ((prediccion.CantidadPredicha / 7f) > 0f ? (prediccion.CantidadPredicha / 7f) : 1f))
                    : 0;

                string prioridad;
                if (producto.Stock <= producto.StockMinimo || diasParaAgotarse <= 3)
                    prioridad = "Alta";
                else if (diasParaAgotarse <= 7)
                    prioridad = "Media";
                else
                    prioridad = "Baja";

                resultados.Add(new CompraOptimaResult
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.Nombre,
                    CantidadSugerida = prediccion.CantidadSugeridaCompra,
                    CostoEstimado = producto.PrecioCompra * prediccion.CantidadSugeridaCompra,
                    UtilidadEsperada = (producto.PrecioMenor - producto.PrecioCompra) * prediccion.CantidadSugeridaCompra,
                    DiasParaAgotarse = diasParaAgotarse,
                    Prioridad = prioridad,
                    Justificacion = $"{prediccion.Recomendacion}. Stock actual: {producto.Stock}, Mínimo: {producto.StockMinimo}"
                });
            }

            return resultados
                .OrderByDescending(r => r.Prioridad == "Alta")
                .ThenByDescending(r => r.Prioridad == "Media")
                .ThenBy(r => r.DiasParaAgotarse)
                .ToList();
        }

        /// <summary>
        /// Obtiene los productos más vendidos en un período
        /// </summary>
        public async Task<List<ProductoRecomendacionML>> ObtenerProductosMasVendidosAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var detallesVenta = await _context.DetallesVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta.FechaVenta >= fechaInicio && d.Venta.FechaVenta <= fechaFin)
                .ToListAsync();

            var productosMasVendidos = detallesVenta
                .GroupBy(d => d.IdProducto)
                .Select(g => new
                {
                    IdProducto = g.Key,
                    CantidadTotal = g.Sum(d => d.Cantidad),
                    VentasPromedio = g.Average(d => d.Cantidad)
                })
                .OrderByDescending(x => x.CantidadTotal)
                .Take(20)
                .ToList();

            var resultados = new List<ProductoRecomendacionML>();
            foreach (var prod in productosMasVendidos)
            {
                var producto = await _context.Productos.FindAsync(prod.IdProducto);
                if (producto != null)
                {
                    resultados.Add(new ProductoRecomendacionML
                    {
                        IdProducto = producto.IdProducto,
                        Nombre = producto.Nombre,
                        PrincipioActivo = producto.PrincipioActivo ?? "N/A",
                        Laboratorio = producto.Laboratorio ?? "N/A",
                        IdCategoria = producto.IdCategoria,
                        PrecioMenor = (float)producto.PrecioMenor,
                        Stock = (float)producto.Stock,
                        VentasPromedio = (float)prod.VentasPromedio
                    });
                }
            }

            return resultados;
        }

        /// <summary>
        /// Obtiene productos de lenta rotación
        /// </summary>
        public async Task<List<ProductoRecomendacionML>> ObtenerProductosLentaRotacionAsync()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Where(p => p.Estado == "Activo" && p.Stock > 0)
                .ToListAsync();

            var fechaInicio = DateTime.Now.AddDays(-30);
            var ventasRecientes = await _context.DetallesVenta
                .Include(d => d.Venta)
                .Where(d => d.Venta.FechaVenta >= fechaInicio)
                .ToListAsync();

            var resultados = new List<ProductoRecomendacionML>();
            foreach (var producto in productos)
            {
                var ventasProducto = ventasRecientes
                    .Where(d => d.IdProducto == producto.IdProducto)
                    .Sum(d => d.Cantidad);

                // Productos con menos de 2 ventas en 30 días y stock mayor a 10
                if (ventasProducto < 2 && producto.Stock > 10)
                {
                    resultados.Add(new ProductoRecomendacionML
                    {
                        IdProducto = producto.IdProducto,
                        Nombre = producto.Nombre,
                        PrincipioActivo = producto.PrincipioActivo ?? "N/A",
                        Laboratorio = producto.Laboratorio ?? "N/A",
                        IdCategoria = producto.IdCategoria,
                        PrecioMenor = (float)producto.PrecioMenor,
                        Stock = (float)producto.Stock,
                        VentasPromedio = (float)ventasProducto / 30f
                    });
                }
            }

            return resultados.OrderByDescending(r => r.Stock).ToList();
        }
    }
}