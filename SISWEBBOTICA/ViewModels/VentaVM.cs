using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <-- ¡Añadir este using!
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class VentaVM
    {
        public int? IdCliente { get; set; }

        [ValidateNever] // <-- AÑADIR AQUÍ
        public SelectList Clientes { get; set; }

        public List<DetalleVentaVM> Detalles { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Subtotal { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal IGV { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
        public int IdMetodoPago { get; set; }

        [ValidateNever] // <-- AÑADIR AQUÍ
        public SelectList MetodosPago { get; set; }

        public string? ReferenciaPago { get; set; }
    }
}