using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;

namespace SISWEBBOTICA.Services
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AppDBContext _context;

        public ClienteRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Clientes
                .Where(c => c.Estado == "Activo")
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Cliente> ObtenerPorIdAsync(int idCliente)
        {
            return await _context.Clientes.FindAsync(idCliente);
        }

        public async Task<Cliente> ObtenerPorRucDniAsync(string rucDni)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.RucDni == rucDni && c.Estado == "Activo");
        }

        public async Task<List<Cliente>> BuscarAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return await ObtenerTodosAsync();

            var terminoLower = termino.ToLower();
            return await _context.Clientes
                .Where(c => c.Estado == "Activo" && (
                    c.Nombre.ToLower().Contains(terminoLower) ||
                    (c.Apellido != null && c.Apellido.ToLower().Contains(terminoLower)) ||
                    c.RucDni.ToLower().Contains(terminoLower) ||
                    (c.Telefono != null && c.Telefono.ToLower().Contains(terminoLower)) ||
                    (c.Email != null && c.Email.ToLower().Contains(terminoLower))))
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Cliente> CrearAsync(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            // Verificar que RUC/DNI no exista
            var existe = await _context.Clientes
                .AnyAsync(c => c.RucDni == cliente.RucDni && c.Estado == "Activo");

            if (existe)
                throw new InvalidOperationException($"El RUC/DNI {cliente.RucDni} ya est� registrado.");

            cliente.FechaRegistro = DateTime.Now;
            cliente.Estado = "Activo";

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task<Cliente> ActualizarAsync(Cliente cliente)
        {
            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            var clienteExistente = await _context.Clientes.FindAsync(cliente.IdCliente);
            if (clienteExistente == null)
                throw new InvalidOperationException("El cliente no existe.");

            // Verificar que RUC/DNI no exista en otro cliente
            var existe = await _context.Clientes
                .AnyAsync(c => c.RucDni == cliente.RucDni && c.IdCliente != cliente.IdCliente && c.Estado == "Activo");

            if (existe)
                throw new InvalidOperationException($"El RUC/DNI {cliente.RucDni} ya est� registrado en otro cliente.");

            clienteExistente.RucDni = cliente.RucDni;
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Apellido = cliente.Apellido;
            clienteExistente.Direccion = cliente.Direccion;
            clienteExistente.Telefono = cliente.Telefono;
            clienteExistente.Email = cliente.Email;
            clienteExistente.EsClienteVIP = cliente.EsClienteVIP;
            clienteExistente.Notas = cliente.Notas;

            _context.Clientes.Update(clienteExistente);
            await _context.SaveChangesAsync();
            return clienteExistente;
        }

        public async Task<bool> EliminarAsync(int idCliente)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                return false;

            // Soft delete: cambiar estado a Inactivo
            cliente.Estado = "Inactivo";
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteAsync(int idCliente)
        {
            return await _context.Clientes
                .AnyAsync(c => c.IdCliente == idCliente && c.Estado == "Activo");
        }

        public async Task<bool> ExisteRucDniAsync(string rucDni)
        {
            return await _context.Clientes
                .AnyAsync(c => c.RucDni == rucDni && c.Estado == "Activo");
        }
    }
}
