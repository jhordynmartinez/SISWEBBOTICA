using SISWEBBOTICA.Models;

namespace SISWEBBOTICA.Services
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> ObtenerTodosAsync();
        Task<Cliente> ObtenerPorIdAsync(int idCliente);
        Task<Cliente> ObtenerPorRucDniAsync(string rucDni);
        Task<List<Cliente>> BuscarAsync(string termino);
        Task<Cliente> CrearAsync(Cliente cliente);
        Task<Cliente> ActualizarAsync(Cliente cliente);
        Task<bool> EliminarAsync(int idCliente);
        Task<bool> ExisteAsync(int idCliente);
        Task<bool> ExisteRucDniAsync(string rucDni);
    }
}
