using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

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

    [HttpGet]
    public async Task<IActionResult> ObtenerUbigeo([FromQuery] string? codigoPadre)
    {
        try
        {
            var resultado = await service.ObtenerUbigeo(codigoPadre);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = ex.Message });
        }
    }
}
