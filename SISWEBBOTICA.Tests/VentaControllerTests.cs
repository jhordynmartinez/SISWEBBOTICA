using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SISWEBBOTICA.Controllers;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System.Security.Claims;

namespace SISWEBBOTICA.Tests
{
    [TestClass]
    public class VentaControllerTests
    {
        private AppDBContext _context;
        private VentaController _controller;

        [TestInitialize]
        public void Setup()
        {
            // 1. Configurar la base de datos en memoria
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDBContext(options);

            // 2. Poblar la base de datos en memoria con datos de prueba
            _context.Categorias.Add(new Categoria { IdCategoria = 1, Nombre = "Analgésicos" });
            _context.UnidadesMedida.Add(new UnidadMedida { IdUnidadMedida = 1, Nombre = "Unidades", Simbolo = "UND" });
            _context.Productos.Add(new Producto { IdProducto = 1, Nombre = "Paracetamol 500mg", Stock = 20, PrecioCompra = 1.0m, PrecioMenor = 1.5m, IdCategoria = 1, IdUnidadMedida = 1 });
            _context.Productos.Add(new Producto { IdProducto = 2, Nombre = "Ibuprofeno 400mg", Stock = 1, PrecioCompra = 1.5m, PrecioMenor = 2.0m, IdCategoria = 1, IdUnidadMedida = 1 });
            _context.Clientes.Add(new Cliente { IdCliente = 1, Nombre = "PÚBLICO GENERAL", RucDni = "00000000" });
            _context.MetodosPago.Add(new MetodoPago { IdMetodoPago = 1, Nombre = "Efectivo" });
            _context.Boticas.Add(new Botica { IdTienda = 1, Nombre = "Mi Botica", PermitirStockNegativo = false });
            _context.Monedas.Add(new Moneda { IdMoneda = 1, Nombre = "SOLES", Simbolo = "S/." });
            _context.SaveChanges();

            // --- INICIO DE LA CORRECCIÓN ---

            // 3. Simular (Mock) UserManager de la forma más simple posible
            var userStoreMock = new Mock<IUserStore<Usuario>>();
            var userManagerMock = new Mock<UserManager<Usuario>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var testUser = new Usuario { Id = 1, UserName = "vendedor_test", Nombre = "Vendedor de Prueba" };

            // Configurar el mock para que devuelva nuestro usuario de prueba
            userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(testUser);

            // 4. Crear la instancia del controlador
            _controller = new VentaController(_context, userManagerMock.Object);

            // 5. Simular el contexto del usuario (HttpContext) que el controlador necesita
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, testUser.Id.ToString()),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // --- FIN DE LA CORRECCIÓN ---
        }

        // --- El resto de los métodos de prueba no necesitan cambios ---

        [TestMethod]
        public async Task Crear_ConStockSuficiente_DebeGuardarVentaYReducirStock()
        {
            // Arrange
            var viewModel = new VentaVM
            {
                IdMetodoPago = 1,
                Total = 6.0m,
                Detalles = new List<DetalleVentaVM>
                {
                    new DetalleVentaVM { IdProducto = 1, Cantidad = 4, Precio = 1.5m, Subtotal = 6.0m }
                }
            };

            // Act
            var result = await _controller.Crear(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = result as RedirectToActionResult;
            Assert.AreEqual("Boleta", redirectResult.ActionName);

            var ventaCreada = await _context.Ventas.FirstOrDefaultAsync();
            Assert.IsNotNull(ventaCreada);
            Assert.AreEqual(6.0m, ventaCreada.TotalPagar);

            var producto = await _context.Productos.FindAsync(1);
            Assert.AreEqual(16, producto.Stock);
        }

        [TestMethod]
        public async Task Crear_ConStockInsuficienteYBloqueoActivado_DebeRetornarVistaConError()
        {
            // Arrange
            var viewModel = new VentaVM
            {
                IdMetodoPago = 1,
                Total = 6.0m,
                Detalles = new List<DetalleVentaVM>
                {
                    new DetalleVentaVM { IdProducto = 2, Cantidad = 3, Precio = 2.0m, Subtotal = 6.0m }
                }
            };

            // Act
            var result = await _controller.Crear(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.Values.Any(v => v.Errors.Any(e => e.ErrorMessage.Contains("Stock insuficiente"))));

            var producto = await _context.Productos.FindAsync(2);
            Assert.AreEqual(1, producto.Stock);
        }

        [TestMethod]
        public async Task Crear_ConStockInsuficienteYPermitirNegativo_DebeGuardarVentaYDejarStockNegativo()
        {
            // Arrange
            var botica = await _context.Boticas.FirstAsync();
            botica.PermitirStockNegativo = true;
            _context.SaveChanges();

            var viewModel = new VentaVM
            {
                IdMetodoPago = 1,
                Total = 10.0m,
                Detalles = new List<DetalleVentaVM>
                {
                    new DetalleVentaVM { IdProducto = 2, Cantidad = 5, Precio = 2.0m, Subtotal = 10.0m }
                }
            };

            // Act
            var result = await _controller.Crear(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var producto = await _context.Productos.FindAsync(2);
            Assert.AreEqual(-4, producto.Stock);
        }

        [TestMethod]
        public async Task Crear_SinProductos_DebeRetornarVistaConError()
        {
            // Arrange
            var viewModel = new VentaVM
            {
                IdMetodoPago = 1,
                Detalles = new List<DetalleVentaVM>()
            };

            // Act
            var result = await _controller.Crear(viewModel);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.Values.Any(v => v.Errors.Any(e => e.ErrorMessage.Contains("Debe seleccionar al menos un medicamento"))));
        }
    }
}