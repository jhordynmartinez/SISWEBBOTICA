using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels; // Asegúrate que LoginVM y RegistroVM estén aquí
using System.Linq;
using System.Threading.Tasks;

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

        // POST: /Cuenta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // El método PasswordSignInAsync se encarga de todo: busca el usuario, hashea la contraseña y la compara.
                var result = await _signInManager.PasswordSignInAsync(model.EmailOrUsername, model.Password, isPersistent: true, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Si hay una URL de retorno, redirige allí, si no, a Home/Index.
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: /Cuenta/Registro
        public async Task<IActionResult> Registro()
        {
            // Crear roles si no existen
            if (!await _roleManager.RoleExistsAsync("Administrador"))
            {
                await _roleManager.CreateAsync(new TipoUsuario("Administrador"));
            }
            if (!await _roleManager.RoleExistsAsync("Vendedor"))
            {
                await _roleManager.CreateAsync(new TipoUsuario("Vendedor"));
            }

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

                    // CreateAsync se encarga de hashear la contraseña
                    var result = await _userManager.CreateAsync(usuario, model.Contrasena);

                    if (result.Succeeded)
                    {
                        // Asignar el rol al nuevo usuario
                        await _userManager.AddToRoleAsync(usuario, model.RolSeleccionado);
                        return RedirectToAction("Login", "Cuenta");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            // Si algo falla, recargamos la data para la vista
            var adminCheck = (await _userManager.GetUsersInRoleAsync("Administrador")).Any();
            ViewBag.PermitirAdmin = !adminCheck;
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.TiposUsuario = new SelectList(roles, "Name", "Name", model.RolSeleccionado);
            return View(model);
        }

        // POST: /Cuenta/Logout
        [HttpPost]
        [Authorize] // Solo usuarios logueados pueden cerrar sesión
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
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}