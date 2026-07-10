using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

[ApiController]
[Route("api/catalogos")]
public sealed class CatalogosController : ControllerBase
{
    [HttpGet("tipos-persona")]
    public async Task<IActionResult> TiposPersona([FromServices] TipoPersonaService s) => Ok(await s.ObtenerTiposPersonaAsync());
    [HttpGet("tipos-documento")]
    public async Task<IActionResult> TiposDocumento([FromServices] TipoDocumentoService s) => Ok(await s.ObtenerTiposDocumentoAsync());
    [HttpGet("tipos-documento-persona")]
    public async Task<IActionResult> TiposDocumentoPersona([FromServices] TipoDocPerService s) => Ok(await s.ObtenerTiposDocPerAsync());
    [HttpGet("estados")]
    public async Task<IActionResult> Estados([FromServices] EstadoService s) => Ok(await s.ObtenerEstadosAsync());
}
