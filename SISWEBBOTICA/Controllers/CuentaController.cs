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
            // --- INICIO DE LA CORRECCIÓN ---
            // Limpiar el returnUrl si contiene la palabra "Error" para romper el bucle
            if (!string.IsNullOrEmpty(returnUrl) && (returnUrl.Contains("Error") || returnUrl.Contains("AccesoDenegado")))
            {
                returnUrl = null;
            }
            // --- FIN DE LA CORRECCIÓN ---

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Cuenta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
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

                var result = await _signInManager.PasswordSignInAsync(usuario, model.Password, isPersistent: true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError(string.Empty, "Contraseña incorrecta.");
                return View(model);
            }
            return View(model);
        }

        // GET: /Cuenta/Registro
        public async Task<IActionResult> Registro(string returnUrl = null)
        {
            // --- INICIO DE LA CORRECCIÓN ---
            if (!string.IsNullOrEmpty(returnUrl) && (returnUrl.Contains("Error") || returnUrl.Contains("AccesoDenegado")))
            {
                returnUrl = null;
            }
            // --- FIN DE LA CORRECCIÓN ---

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
                        UserName = model.Login,
                        Email = model.Login,
                        Nombre = model.Nombre,
                        Estado = "Activo",
                        FechaRegistro = DateTime.Now
                    };

                    var result = await _userManager.CreateAsync(usuario, model.Contrasena);

                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(usuario, model.RolSeleccionado);
                        TempData["SuccessMessage"] = "Usuario registrado correctamente. Ahora puede iniciar sesión.";
                        return RedirectToAction("Login", "Cuenta");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            await CrearRolesSiNoExisten();
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

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
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