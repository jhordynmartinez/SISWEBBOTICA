using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public interface IUnidadMedidaRepository
    {
        Task<List<UnidadMedida>> GetAllAsync();
    }
}