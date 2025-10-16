using Microsoft.AspNetCore.Mvc;
using Moq;
using SISWEBBOTICA.Controllers;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.Services;
using SISWEBBOTICA.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SISWEBBOTICA.Tests
{
    [TestClass]
    public class ProductoControllerTests
    {
        private Mock<IProductoRepository> _productoRepoMock;
        private Mock<ICategoriaRepository> _categoriaRepoMock;
        private Mock<IUnidadMedidaRepository> _unidadMedidaRepoMock;
        private ProductoController _controller;

        [TestInitialize]
        public void Setup()
        {
            _productoRepoMock = new Mock<IProductoRepository>();
            _categoriaRepoMock = new Mock<ICategoriaRepository>();
            _unidadMedidaRepoMock = new Mock<IUnidadMedidaRepository>();

            // --- INICIO DE LA CORRECCIÓN CLAVE ---
            // Asegurarse de que los repositorios de catálogo siempre devuelvan una lista,
            // aunque esté vacía, para evitar NullReferenceException al recargar la vista.
            _categoriaRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Categoria>());
            _unidadMedidaRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<UnidadMedida>());
            // --- FIN DE LA CORRECCIÓN CLAVE ---

            _controller = new ProductoController(
                _productoRepoMock.Object,
                _categoriaRepoMock.Object,
                _unidadMedidaRepoMock.Object
            );
        }

        [TestMethod]
        public async Task Index_FiltroStockBajo_DebeLlamarAlRepositorioYRetornarVistaConResultados()
        {
            // Arrange
            var productosFalsos = new List<Producto> { new Producto { Nombre = "Test Prod", Stock = 3, Categoria = new Categoria(), UnidadMedida = new UnidadMedida() } };
            _productoRepoMock.Setup(repo => repo.GetProductosAsync(null, "bajo")).ReturnsAsync(productosFalsos);

            // Act
            var result = await _controller.Index(null, "bajo") as ViewResult;
            var model = result.Model as List<Producto>;

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
            _productoRepoMock.Verify(repo => repo.GetProductosAsync(null, "bajo"), Times.Once);
        }

        [TestMethod]
        public async Task Create_ConProductoValido_DebeLlamarAlRepositorioYRedirigir()
        {
            // Arrange
            var viewModel = new ProductoVM
            {
                Producto = new Producto
                {
                    Nombre = "Nuevo Prod",
                    CodigoBarras = "123",
                    PrecioCompra = 1,
                    PrecioMenor = 1,
                    PrecioMayor = 1,
                    Stock = 1,
                    StockMinimo = 1,
                    IdCategoria = 1, // Es requerido
                    IdUnidadMedida = 1 // Es requerido
                }
            };

            // Simulamos que el producto no existe
            _productoRepoMock.Setup(repo => repo.ProductoDuplicadoExistsAsync(It.IsAny<string>(), It.IsAny<string>(), 0)).ReturnsAsync(false);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            _productoRepoMock.Verify(repo => repo.CreateProductoAsync(viewModel.Producto), Times.Once);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        }

        [TestMethod]
        public async Task Create_ConProductoDuplicado_DebeRetornarVistaConError()
        {
            // Arrange
            var viewModel = new ProductoVM { Producto = new Producto { Nombre = "Duplicado" } };
            _productoRepoMock.Setup(repo => repo.ProductoDuplicadoExistsAsync("Duplicado", null, 0)).ReturnsAsync(true);

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            _productoRepoMock.Verify(repo => repo.CreateProductoAsync(It.IsAny<Producto>()), Times.Never);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(_controller.ModelState.IsValid);
        }
    }
}