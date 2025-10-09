using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // <-- ¡Añadir este using!
using System.ComponentModel.DataAnnotations;

namespace SISWEBBOTICA.ViewModels
{
    public class DetalleVentaVM
    {
        [Required]
        public int IdProducto { get; set; }

        [ValidateNever] // <-- AÑADIR AQUÍ
        public string NombreProducto { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public decimal Cantidad { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public decimal Subtotal { get; set; }
    }
}