using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace SISWEBBOTICA.Controllers
{
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<TipoUsuario> _roleManager;

        public CuentaController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, RoleManager<TipoUsuario> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: /Cuenta/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Cuenta/Login (CORREGIDO Y MEJORADO)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Buscamos al usuario primero para verificar su estado
                var usuario = await _userManager.FindByNameAsync(model.EmailOrUsername);

                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "El usuario no está registrado.");
                    return View(model);
                }

                if (usuario.Estado == "Inactivo")
                {
                    ModelState.AddModelError(string.Empty, "Usuario no autorizado.");
                    return View(model);
                }

                // Intentamos iniciar sesión
                var result = await _signInManager.PasswordSignInAsync(usuario, model.Password, isPersistent: true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }

                // Si el inicio de sesión falla después de las verificaciones, es por la contraseña
                ModelState.AddModelError(string.Empty, "Contraseña incorrecta.");
                return View(model);
            }
            return View(model);
        }

        // GET: /Cuenta/Registro
        public async Task<IActionResult> Registro()
        {
            await CrearRolesSiNoExisten();

            var adminExistente = (await _userManager.GetUsersInRoleAsync("Administrador")).Any();
            ViewBag.PermitirAdmin = !adminExistente;

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.TiposUsuario = new SelectList(roles, "Name", "Name");
            return View();
        }

        // POST: /Cuenta/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroVM model)
        {
            if (ModelState.IsValid)
            {
                var adminExistente = (await _userManager.GetUsersInRoleAsync("Administrador")).Any();
                if (model.RolSeleccionado == "Administrador" && adminExistente)
                {
                    ModelState.AddModelError(string.Empty, "Ya existe un administrador en el sistema.");
                }
                else
                {
                    var usuario = new Usuario
                    {
                        UserName = model.Login, // Identity usa UserName para el login
                        Email = model.Login,    // Y Email para la comunicación
                        Nombre = model.Nombre,
                        Estado = "Activo",
                        FechaRegistro = DateTime.Now
                    };

                    var result = await _userManager.CreateAsync(usuario, model.Contrasena);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(usuario, model.RolSeleccionado);
                        // Opcional: Redirigir con mensaje de éxito
                        TempData["SuccessMessage"] = "Usuario registrado correctamente. Ahora puede iniciar sesión.";
                        return RedirectToAction("Login", "Cuenta");
                    }

                    // Si falla, Identity nos da los errores específicos (ej. "Username 'jperez' is already taken.")
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Si algo falla, recargamos la data para la vista
            await CrearRolesSiNoExisten(); // Nos aseguramos que los roles existan
            var adminCheck = (await _userManager.GetUsersInRoleAsync("Administrador")).Any();
            ViewBag.PermitirAdmin = !adminCheck;
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.TiposUsuario = new SelectList(roles, "Name", "Name", model.RolSeleccionado);
            return View(model);
        }

        // POST: /Cuenta/Logout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Cuenta");
        }

        // GET: /Cuenta/AccesoDenegado
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // --- MÉTODOS AUXILIARES ---
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        private async Task CrearRolesSiNoExisten()
        {
            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                await _roleManager.CreateAsync(new TipoUsuario { Name = "Administrador", Descripcion = "Acceso total al sistema." });
            }
            if (!await _roleManager.RoleExistsAsync("Vendedor"))
            {
                await _roleManager.CreateAsync(new TipoUsuario { Name = "Vendedor", Descripcion = "Acceso limitado a ventas y productos." });
            }
        }
    }
}