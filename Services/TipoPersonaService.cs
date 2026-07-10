using MesaPartesDigital.Data;
using MesaPartesDigital.Models;
using Microsoft.EntityFrameworkCore;

namespace MesaPartesDigital.Services
{
    public class TipoPersonaService
    {
        private readonly ApplicationDbContext _context;

        public TipoPersonaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Este es tu GET a la Base de Datos
        public async Task<List<TipoPersona>> ObtenerTiposPersonaAsync()
        {
            return await _context.TipoPersonas.ToListAsync();
        }
    }
}