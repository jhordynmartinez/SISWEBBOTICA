using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SISWEBBOTICA.Models;
using System.Collections.Generic;

namespace SISWEBBOTICA.ViewModels
{
    public class ProductoVM
    {
        public Producto Producto { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> CategoriasList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> UnidadesMedidaList { get; set; }
    }
}