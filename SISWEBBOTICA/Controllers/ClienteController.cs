using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using SISWEBBOTICA.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly AppDBContext _context;

        public ClienteController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /Cliente - CRUD completo para Admin y Vendedor
        [Authorize(Roles = "Administrador,Vendedor")]
        public async Task<IActionResult> Index(string buscar)
        {
            var clientesQuery = _context.Clientes
                .Where(c => c.Nombre != "PÚBLICO GENERAL" && c.Estado == "Activo")
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                clientesQuery = clientesQuery.Where(c =>
                    c.Nombre.Contains(buscar) ||
                    (c.Apellido != null && c.Apellido.Contains(buscar)) ||
                    c.RucDni.Contains(buscar) ||
                    (c.Telefono != null && c.Telefono.Contains(buscar)));
            }

            var clientes = await clientesQuery
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewData["Buscar"] = buscar;
            return View(clientes);
        }

        // GET: /Cliente/Create
        [Authorize(Roles = "Administrador,Vendedor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Cliente/Create
        [Authorize(Roles = "Administrador,Vendedor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClienteCreateVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validar que el RUC/DNI no exista
            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.RucDni == model.RucDni);

            if (clienteExistente != null)
            {
                ModelState.AddModelError("RucDni", "Ya existe un cliente con este RUC/DNI.");
                return View(model);
            }

            var cliente = new Cliente
            {
                RucDni = model.RucDni,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Direccion = model.Direccion,
                Telefono = model.Telefono,
                Email = model.Email,
                FechaRegistro = DateTime.Now,
                Estado = "Activo",
                EsClienteVIP = model.EsClienteVIP,
                Notas = model.Notas
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cliente '{cliente.Nombre}' registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cliente/Edit/5
        [Authorize(Roles = "Administrador,Vendedor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            var model = new ClienteEditVM
            {
                IdCliente = cliente.IdCliente,
                RucDni = cliente.RucDni,
                Nombre = cliente.Nombre,
                Apellido = cliente.Apellido,
                Direccion = cliente.Direccion,
                Telefono = cliente.Telefono,
                Email = cliente.Email,
                Estado = cliente.Estado,
                FechaRegistro = cliente.FechaRegistro,
                EsClienteVIP = cliente.EsClienteVIP,
                Notas = cliente.Notas
            };

            return View(model);
        }

        // POST: /Cliente/Edit/5
        [Authorize(Roles = "Administrador,Vendedor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClienteEditVM model)
        {
            if (id != model.IdCliente)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                {
                    return NotFound();
                }

                // Validar que el RUC/DNI no exista en otro cliente
                var clienteExistente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.RucDni == model.RucDni && c.IdCliente != id);

                if (clienteExistente != null)
                {
                    ModelState.AddModelError("RucDni", "Ya existe otro cliente con este RUC/DNI.");
                    return View(model);
                }

                cliente.RucDni = model.RucDni;
                cliente.Nombre = model.Nombre;
                cliente.Apellido = model.Apellido;
                cliente.Direccion = model.Direccion;
                cliente.Telefono = model.Telefono;
                cliente.Email = model.Email;
                cliente.EsClienteVIP = model.EsClienteVIP;
                cliente.Notas = model.Notas;

                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Cliente '{cliente.Nombre}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Error de concurrencia. El cliente fue modificado por otra persona.");
                return View(model);
            }
        }

        // GET: /Cliente/Delete/5
        [Authorize(Roles = "Administrador,Vendedor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Cliente/Delete/5
        [Authorize(Roles = "Administrador,Vendedor")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            // Validar que el cliente no tenga ventas
            var ventasAsociadas = await _context.Ventas
                .Where(v => v.IdCliente == id)
                .CountAsync();

            if (ventasAsociadas > 0)
            {
                TempData["ErrorMessage"] = $"No se puede eliminar el cliente '{cliente.Nombre}' porque tiene {ventasAsociadas} venta(s) asociada(s).";
                return RedirectToAction(nameof(Index));
            }

            // Soft delete: cambiar estado a Inactivo
            cliente.Estado = "Inactivo";
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cliente '{cliente.Nombre}' eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cliente/BuscarJson?termino=xxx - Para búsqueda rápida en ventas
        [HttpGet]
        public async Task<IActionResult> BuscarJson(string termino)
        {
            try
            {
                if (string.IsNullOrEmpty(termino) || termino.Length < 1)
                {
                    // Si no hay término, devolver todos los clientes activos
                    var todosClientes = await _context.Clientes
                        .Where(c => c.Nombre != "PÚBLICO GENERAL" && c.Estado == "Activo")
                        .Select(c => new
                        {
                            id = c.IdCliente,
                            label = $"{c.Nombre} {c.Apellido} - {c.RucDni}",
                            value = c.IdCliente,
                            nombre = c.Nombre,
                            apellido = c.Apellido ?? "",
                            rucDni = c.RucDni,
                            telefono = c.Telefono ?? "",
                            email = c.Email ?? "",
                            direccion = c.Direccion ?? ""
                        })
                        .OrderBy(c => c.nombre)
                        .ToListAsync();

                    return Json(todosClientes);
                }

                var terminoLower = termino.ToLower();
                var clientes = await _context.Clientes
                    .Where(c => c.Nombre != "PÚBLICO GENERAL" && c.Estado == "Activo" && (
                        c.Nombre.ToLower().Contains(terminoLower) ||
                        c.RucDni.ToLower().Contains(terminoLower) ||
                        (c.Apellido != null && c.Apellido.ToLower().Contains(terminoLower)) ||
                        (c.Telefono != null && c.Telefono.ToLower().Contains(terminoLower))))
                    .Select(c => new
                    {
                        id = c.IdCliente,
                        label = $"{c.Nombre} {c.Apellido} - {c.RucDni}",
                        value = c.IdCliente,
                        nombre = c.Nombre,
                        apellido = c.Apellido ?? "",
                        rucDni = c.RucDni,
                        telefono = c.Telefono ?? "",
                        email = c.Email ?? "",
                        direccion = c.Direccion ?? ""
                    })
                    .OrderBy(c => c.nombre)
                    .Take(10)
                    .ToListAsync();

                return Json(clientes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BuscarJson: {ex.ToString()}");
                return Json(new List<object>());
            }
        }

        // POST: /Cliente/RegistrarRapido - Registrar cliente desde modal de ventas
        [HttpPost]
        public async Task<IActionResult> RegistrarRapido([FromBody] ClienteCreateVM model)
        {
            // Validar campos obligatorios manualmente
            if (model == null || string.IsNullOrWhiteSpace(model.RucDni))
            {
                return BadRequest(new { success = false, message = "El RUC/DNI es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                return BadRequest(new { success = false, message = "El nombre es obligatorio." });
            }

            if (string.IsNullOrWhiteSpace(model.Apellido))
            {
                return BadRequest(new { success = false, message = "Los apellidos son obligatorios." });
            }

            if (string.IsNullOrWhiteSpace(model.Telefono))
            {
                return BadRequest(new { success = false, message = "El teléfono es obligatorio." });
            }

            try
            {
                // Validar que el RUC/DNI no exista
                var clienteExistente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.RucDni == model.RucDni.Trim());

                if (clienteExistente != null)
                {
                    return BadRequest(new { success = false, message = "Ya existe un cliente con este RUC/DNI." });
                }

                var cliente = new Cliente
                {
                    RucDni = model.RucDni.Trim(),
                    Nombre = model.Nombre.Trim(),
                    Apellido = model.Apellido.Trim(),
                    Telefono = model.Telefono.Trim(),
                    Email = model.Email?.Trim(),
                    Direccion = model.Direccion?.Trim(),
                    FechaRegistro = DateTime.Now,
                    Estado = "Activo",
                    EsClienteVIP = model.EsClienteVIP,
                    Notas = model.Notas?.Trim()
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"Cliente '{cliente.Nombre}' registrado exitosamente.",
                    cliente = new
                    {
                        id = cliente.IdCliente,
                        nombre = cliente.Nombre,
                        apellido = cliente.Apellido,
                        rucDni = cliente.RucDni,
                        telefono = cliente.Telefono,
                        email = cliente.Email,
                        direccion = cliente.Direccion
                    }
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error DB al registrar cliente: {dbEx.ToString()}");
                return StatusCode(500, new { success = false, message = "Error de base de datos: " + (dbEx.InnerException?.Message ?? dbEx.Message) });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al registrar cliente: {ex.ToString()}");
                return StatusCode(500, new { success = false, message = "Error al registrar el cliente: " + ex.Message });
            }
        }
    }
}
