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
    public async Task<IActionResult> Juridica(int tipoDocumento, string documento, [FromServices] ApplicationDbContext db)
        => Ok((await db.ObtenerPersonaJuridicaPorRucAsync(tipoDocumento, documento)).FirstOrDefault());
}
