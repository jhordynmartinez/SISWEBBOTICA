using SISWEBBOTICA.Models;
using System.Collections.Generic;

namespace SISWEBBOTICA.ViewModels
{
    public class BoletaVM
    {
        public Venta Venta { get; set; }
        public Botica Tienda { get; set; }
        public List<DetalleVenta> Detalles { get; set; }
    }
}