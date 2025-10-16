using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> GetAllAsync();
    }
}