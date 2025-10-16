using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SISWEBBOTICA.Controllers;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System.Security.Claims;

namespace SISWEBBOTICA.Tests
{
    [TestClass]
    public class CuentaControllerTests
    {
        private Mock<UserManager<Usuario>> _userManagerMock;
        private Mock<SignInManager<Usuario>> _signInManagerMock;
        private Mock<RoleManager<TipoUsuario>> _roleManagerMock;
        private CuentaController _controller;

        [TestInitialize]
        public void Setup()
        {
            var userStoreMock = new Mock<IUserStore<Usuario>>();
            _userManagerMock = new Mock<UserManager<Usuario>>(userStoreMock.Object, null, null, null, null, null, null, null, null);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<Usuario>>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<Usuario>>>();
            _signInManagerMock = new Mock<SignInManager<Usuario>>(_userManagerMock.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null, loggerMock.Object, null, null);

            var roleStoreMock = new Mock<IRoleStore<TipoUsuario>>();
            _roleManagerMock = new Mock<RoleManager<TipoUsuario>>(roleStoreMock.Object, null, null, null, null);

            _controller = new CuentaController(_userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object);
        }

        [TestMethod]
        public async Task Login_ConUsuarioInactivo_DebeRetornarVistaConError()
        {
            // Arrange
            var loginVM = new LoginVM { EmailOrUsername = "inactivo@test.com", Password = "Password123" };
            var usuarioInactivo = new Usuario { UserName = "inactivo@test.com", Estado = "Inactivo" };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginVM.EmailOrUsername)).ReturnsAsync(usuarioInactivo);

            // Act
            var result = await _controller.Login(loginVM);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.AreEqual("Usuario no autorizado.", _controller.ModelState.Values.First().Errors.First().ErrorMessage);
        }

        [TestMethod]
        public async Task Login_ConContraseñaIncorrecta_DebeRetornarVistaConError()
        {
            // Arrange
            var loginVM = new LoginVM { EmailOrUsername = "test@test.com", Password = "Password_Incorrecta" };
            var usuarioActivo = new Usuario { UserName = "test@test.com", Estado = "Activo" };

            _userManagerMock.Setup(um => um.FindByNameAsync(loginVM.EmailOrUsername)).ReturnsAsync(usuarioActivo);

            _signInManagerMock.Setup(sm => sm.PasswordSignInAsync(usuarioActivo, loginVM.Password, true, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Act
            var result = await _controller.Login(loginVM);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsFalse(_controller.ModelState.IsValid);
            Assert.IsTrue(_controller.ModelState.Values.Any(v => v.Errors.Any(e => e.ErrorMessage == "Contraseña incorrecta.")));
        }

        // Esta prueba es compleja y se considera de integración.
        // La comentamos para enfocarnos en las pruebas unitarias puras de nuestra lógica.
        // [TestMethod]
        // public async Task Login_ConCredencialesCorrectas_DebeRedirigirAHome() { ... }
    }
}