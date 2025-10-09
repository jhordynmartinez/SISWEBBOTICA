using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace SISWEBBOTICA.Models
{
    public class Producto
    {
        public Producto()
        {
            this.DetallesVenta = new HashSet<DetalleVenta>();
            this.DetallesCompra = new HashSet<DetalleCompra>();
            this.DetallesCotizacion = new HashSet<DetalleCotizacion>();
        }

        [Key]
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Categoría")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una unidad de medida.")]
        [Display(Name = "Unidad de Medida")]
        public int IdUnidadMedida { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(255)]
        [Display(Name = "Nombre del Producto")]
        public string Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El código de barras es obligatorio.")]
        [StringLength(60)]
        [Display(Name = "Código de Barras")]
        public string CodigoBarras { get; set; }

        [StringLength(255)]
        public string? Laboratorio { get; set; }

        [StringLength(255)]
        [Display(Name = "Registro Sanitario")]
        public string? RegistroSanitario { get; set; }

        [StringLength(255)]
        [Display(Name = "Principio Activo")]
        public string? PrincipioActivo { get; set; }

        [StringLength(255)]
        [Display(Name = "Presentación")]
        public string? Presentacion { get; set; }

        [StringLength(80)]
        public string? Lote { get; set; }

        [StringLength(80)]
        [Display(Name = "Ubicación")]
        public string? Ubicacion { get; set; }

        [Required(ErrorMessage = "El precio de compra es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio de compra no puede ser negativo.")]
        [Display(Name = "Precio de Compra")]
        public decimal PrecioCompra { get; set; }

        [Required(ErrorMessage = "El precio al por menor es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio al por menor no puede ser negativo.")]
        [Display(Name = "Precio al por Menor")]
        public decimal PrecioMenor { get; set; }

        [Required(ErrorMessage = "El precio al por mayor es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio al por mayor no puede ser negativo.")]
        [Display(Name = "Precio al por Mayor")]
        public decimal PrecioMayor { get; set; }

        [Required(ErrorMessage = "El stock inicial es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El stock no puede ser un número negativo.")]
        [Display(Name = "Stock")]
        public decimal Stock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El stock mínimo no puede ser un número negativo.")]
        [Display(Name = "Stock Mínimo")]
        public decimal StockMinimo { get; set; }

        [Display(Name = "Fecha de Vencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Activo";

        [ForeignKey("IdCategoria")]
        [ValidateNever]
        public virtual Categoria Categoria { get; set; }

        [ForeignKey("IdUnidadMedida")]
        [ValidateNever]
        public virtual UnidadMedida UnidadMedida { get; set; }

        public virtual ICollection<DetalleVenta> DetallesVenta { get; set; }
        public virtual ICollection<DetalleCompra> DetallesCompra { get; set; }
        public virtual ICollection<DetalleCotizacion> DetallesCotizacion { get; set; }
    }
}