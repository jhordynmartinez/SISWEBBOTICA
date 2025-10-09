using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class DashboardVM
    {
        // --- MÉTRICAS PRINCIPALES (Para Admin y Vendedor) ---
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal VentasHoy { get; set; }
        public int NumeroVentasHoy { get; set; }
        public int TotalProductos { get; set; }
        public int TotalClientes { get; set; }

        // --- LISTAS DE ALERTA (Solo para Admin) ---
        public List<Producto> ProductosBajoStock { get; set; }
        public List<Producto> ProductosProximosAVencer { get; set; }

        // --- TOP 5 (Solo para Admin) ---
        public Dictionary<string, decimal> TopProductosVendidos { get; set; }

        public DashboardVM()
        {
            ProductosBajoStock = new List<Producto>();
            ProductosProximosAVencer = new List<Producto>();
            TopProductosVendidos = new Dictionary<string, decimal>();
        }
    }
}