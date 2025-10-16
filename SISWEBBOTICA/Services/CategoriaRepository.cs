using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDBContext _context;
        public CategoriaRepository(AppDBContext context) { _context = context; }

        public async Task<List<Categoria>> GetAllAsync()
        {
            return await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        }
    }
}