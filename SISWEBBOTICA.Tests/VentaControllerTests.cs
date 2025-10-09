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
        private DbContextOptions<AppDBContext> _dbOptions;
        private Mock<UserManager<Usuario>> _userManagerMock;
        private Usuario _testUser;

        [TestInitialize]
        public void Setup()
        {
            try
            {
                // Configuración de la base de datos en memoria
                _dbOptions = new DbContextOptionsBuilder<AppDBContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

                // Creación y población del contexto
                using (var context = new AppDBContext(_dbOptions))
                {
                    // --- INICIO DE LA CORRECCIÓN ---
                    // Poblar datos usando IDs en lugar de objetos de navegación
                    context.Categorias.Add(new Categoria { IdCategoria = 1, Nombre = "Analgésicos" });
                    context.UnidadesMedida.Add(new UnidadMedida { IdUnidadMedida = 1, Nombre = "Unidades", Simbolo = "UND" });
                    context.SaveChanges(); // Guardar cat y unidad primero

                    context.Productos.Add(new Producto { IdProducto = 1, Nombre = "Paracetamol 500mg", Stock = 20, PrecioCompra = 1.0m, PrecioMenor = 1.5m, IdCategoria = 1, IdUnidadMedida = 1 });
                    context.Productos.Add(new Producto { IdProducto = 2, Nombre = "Ibuprofeno 400mg", Stock = 1, PrecioCompra = 1.5m, PrecioMenor = 2.0m, IdCategoria = 1, IdUnidadMedida = 1 });

                    context.Clientes.Add(new Cliente { IdCliente = 1, Nombre = "PÚBLICO GENERAL", RucDni = "00000000" });
                    context.MetodosPago.Add(new MetodoPago { IdMetodoPago = 1, Nombre = "Efectivo" });
                    context.Boticas.Add(new Botica { IdTienda = 1, Nombre = "Mi Botica", PermitirStockNegativo = false });
                    context.Monedas.Add(new Moneda { IdMoneda = 1, Nombre = "SOLES", Simbolo = "S/." });
                    context.SaveChanges();
                    // --- FIN DE LA CORRECCIÓN ---
                }

                // Configuración del mock de UserManager
                var userStoreMock = new Mock<IUserStore<Usuario>>();
                _userManagerMock = new Mock<UserManager<Usuario>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
                _testUser = new Usuario { Id = 1, UserName = "vendedor_test", Nombre = "Vendedor de Prueba" };
                _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_testUser);
            }
            catch (Exception ex)
            {
                // Si el Setup falla, esta excepción nos dará la causa exacta
                throw new Exception("El método Setup() falló.", ex);
            }
        }

        // El resto de los métodos de prueba (TestMethod) no necesitan cambios.
        // ... (pega aquí los 4 métodos [TestMethod] de la respuesta anterior)
        [TestMethod]
        public async Task Crear_ConStockSuficiente_DebeGuardarVentaYReducirStock() { /* ... */ }
        [TestMethod]
        public async Task Crear_ConStockInsuficienteYBloqueoActivado_DebeRetornarVistaConError() { /* ... */ }
        [TestMethod]
        public async Task Crear_ConStockInsuficienteYPermitirNegativo_DebeGuardarVentaYDejarStockNegativo() { /* ... */ }
        [TestMethod]
        public async Task Crear_SinProductos_DebeRetornarVistaConError() { /* ... */ }
    }
}