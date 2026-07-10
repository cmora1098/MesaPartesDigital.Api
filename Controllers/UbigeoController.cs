using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

[ApiController]
[Route("api/ubigeo")]
public sealed class UbigeoController(UbigeoService service) : ControllerBase
{
    [HttpGet("departamentos")]
    public async Task<IActionResult> Departamentos()
    {
        return Ok(await service.ObtenerDepartamentosAsync());
    }

    [HttpGet("provincias/{codigo}")]
    public async Task<IActionResult> Provincias(string codigo)
    {
        try
        {
            return Ok(await service.ObtenerProvinciasAsync(codigo));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("distritos/{codigo}")]
    public async Task<IActionResult> Distritos(string codigo)
    {
        try
        {
            return Ok(await service.ObtenerDistritosAsync(codigo));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
