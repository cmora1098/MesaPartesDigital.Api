using MesaPartesDigital.Data;
using MesaPartesDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace MesaPartesDigital.Services
{
    public class EstadoService
    {
        private readonly ApplicationDbContext _context;

        public EstadoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🟢 TU CUARTO GET
        public async Task<List<Estado>> ObtenerEstadosAsync()
        {
            return await _context.Estados.ToListAsync();
        }
    }
}