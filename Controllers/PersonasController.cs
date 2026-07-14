using MesaPartesDigital.Data;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

[ApiController]
[Route("api/personas")]
public sealed class PersonasController : ControllerBase
{
    [HttpGet("natural")]
    public async Task<IActionResult> Natural(int tipoDocumento, string documento, [FromServices] TipoDocPerService s)
        => Ok(await s.BuscarPersonaPorDocumentoAsync(tipoDocumento, documento));

    [HttpGet("juridica")]
    public async Task<IActionResult> Juridica(string documento, [FromServices] ContribuyenteService service)
    {
        var resultado = await service.ObtenerPorRucAsync(documento);

        if (resultado == null)
            return NotFound("Contribuyente no encontrado.");

        return Ok(resultado);
    }
}
