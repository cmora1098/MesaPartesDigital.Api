using MesaPartesDigital.Api.Models;

namespace MesaPartesDigital.Api.Services
{
    public interface IPersonaService
    {
        Task<PersonaDto> ObtenerPorIdAsync(int iCodPer);
        Task<bool> ActualizarAsync(PersonaDto persona);
    }
}
