using Microsoft.EntityFrameworkCore;
using SISWEBBOTICA.Data;
using SISWEBBOTICA.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISWEBBOTICA.Services
{
    public class UnidadMedidaRepository : IUnidadMedidaRepository
    {
        private readonly AppDBContext _context;
        public UnidadMedidaRepository(AppDBContext context) { _context = context; }

        public async Task<List<UnidadMedida>> GetAllAsync()
        {
            return await _context.UnidadesMedida.OrderBy(u => u.Nombre).ToListAsync();
        }
    }
}