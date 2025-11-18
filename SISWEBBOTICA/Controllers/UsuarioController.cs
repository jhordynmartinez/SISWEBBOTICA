using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        // --- INICIO DE LA CORRECCIÓN ---
        // Se elimina la dependencia _context que no se usaba.
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<TipoUsuario> _roleManager;

        public UsuarioController(UserManager<Usuario> userManager, RoleManager<TipoUsuario> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // --- FIN DE LA CORRECCIÓN ---

        // GET: Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            var usuarioVMs = new List<UsuarioVM>();
            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                usuarioVMs.Add(new UsuarioVM { Usuario = usuario, Rol = roles.FirstOrDefault() });
            }
            return View(usuarioVMs);
        }

        // GET: Usuario/Create
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name");
            return View();
        }

        // POST: Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateVM model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Email = model.Email,
                    UserName = model.UserName,
                    Estado = "Activo",
                    FechaRegistro = System.DateTime.Now
                };
                var result = await _userManager.CreateAsync(usuario, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(usuario, model.Rol);
                    TempData["SuccessMessage"] = "Usuario creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            ViewBag.Roles = new SelectList(_roleManager.Roles.ToList(), "Name", "Name", model.Rol);
            return View(model);
        }

        // POST: Usuario/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Estado = (usuario.Estado == "Activo") ? "Inactivo" : "Activo";
            var result = await _userManager.UpdateAsync(usuario);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"El estado del usuario {usuario.UserName} ha sido cambiado a {usuario.Estado}.";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo cambiar el estado del usuario.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    // El ViewModel se puede mantener aquí o moverlo a la carpeta ViewModels si prefieres
    public class UsuarioVM
    {
        public Usuario Usuario { get; set; }
        public string Rol { get; set; }
    }
}