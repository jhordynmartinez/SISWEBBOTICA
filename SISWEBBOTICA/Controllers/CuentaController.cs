using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System.Security.Claims;
using BCrypt.Net;



namespace SISWEBBOTICA.Controllers
{
    [AllowAnonymous]
    public class CuentaController : Controller
    {
        private readonly AppDBContext _context;

        public CuentaController(AppDBContext context)
        {
            _context = context;
        }

       // GET: /Cuenta/Login
        public IActionResult Login()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        // POST: /Cuenta/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.TipoUsuario)
                    .FirstOrDefaultAsync(u => u.Login == model.EmailOrUsername);

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Contrasena))
                {
                    ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim("Login", usuario.Login),
                    new Claim(ClaimTypes.Role, usuario.TipoUsuario.Descripcion)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
        // GET: /Cuenta/Registro
        public IActionResult Registro()
        {
            // Verificar si ya existe un Administrador
            var adminExistente = _context.Usuarios
                .Include(u => u.TipoUsuario)
                .Any(u => u.TipoUsuario.Descripcion == "Administrador");

            ViewBag.PermitirAdmin = !adminExistente;
            ViewBag.TiposUsuario = _context.TiposUsuario.ToList();
            return View();
        }

        // POST: /Cuenta/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroVM model)
        {
            var adminExistente = _context.Usuarios
                .Include(u => u.TipoUsuario)
                .Any(u => u.TipoUsuario.Descripcion == "Administrador");

            if (!ModelState.IsValid)
            {
                ViewBag.PermitirAdmin = !adminExistente;
                ViewBag.TiposUsuario = _context.TiposUsuario.ToList();
                return View(model);
            }

            // Evitar más de un administrador
            if (model.IdTipoUsuario != 0)
            {
                var tipoUsuario = _context.TiposUsuario
                    .FirstOrDefault(t => t.IdTipoUsuario == model.IdTipoUsuario);

                if (tipoUsuario == null)
                {
                    ModelState.AddModelError("IdTipoUsuario", "Tipo de usuario inválido.");
                    ViewBag.PermitirAdmin = !adminExistente;
                    ViewBag.TiposUsuario = _context.TiposUsuario.ToList();
                    return View(model);
                }

                if (tipoUsuario.Descripcion == "Administrador" && adminExistente)
                {
                    ModelState.AddModelError("IdTipoUsuario", "Ya existe un administrador registrado.");
                    ViewBag.PermitirAdmin = false;
                    ViewBag.TiposUsuario = _context.TiposUsuario.ToList();
                    return View(model);
                }
            }

            // Validar que el correo no exista
            if (_context.Usuarios.Any(u => u.Login == model.Login))
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado.");
                ViewBag.PermitirAdmin = !adminExistente;
                ViewBag.TiposUsuario = _context.TiposUsuario.ToList();
                return View(model);
            }

            // Crear usuario
            var nuevo = new Usuario
            {
                Nombre = model.Nombre,
                Login = model.Login, // correo como login
                Contrasena = BCrypt.Net.BCrypt.HashPassword(model.Contrasena),
                Estado = "Activo",
                IdTipoUsuario = model.IdTipoUsuario,
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(nuevo);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        [Route("Cuenta/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

