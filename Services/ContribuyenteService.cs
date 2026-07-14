using MesaPartesDigital.Data;
using MesaPartesDigital.Models;
using Microsoft.EntityFrameworkCore; // Asegúrate de tener este using

public class ContribuyenteService
{
    private readonly ApplicationDbContext _db;

    public ContribuyenteService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ContribuyenteDto?> ObtenerPorRucAsync(string ruc)
    {
        // EF Core ahora encontrará 'ruc' y 'nombre_razon_social' en el resultado
        var resultado = await _db.Contribuyente
            .FromSqlRaw("EXEC [dbo].[USP_PersonaJuridica_ObtenerPorRUC] @vRuc={0}", ruc)
            .ToListAsync();

        return resultado.FirstOrDefault();
    }
}