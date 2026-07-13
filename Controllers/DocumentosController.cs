using MesaPartesDigital.Models;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

public sealed record RegistroJuridicoRequest(RegistroDocumentoRequest Documento, string RucEmpresa, string RazonSocial);

[ApiController]
[Route("api/documentos")]
public sealed class DocumentosController(DocumentoService service) : ControllerBase
{
    [HttpGet("tipos-documento")]
    public async Task<IActionResult> GetTiposDocumento() => Ok(await service.ObtenerTiposDocumentoActivosAsync());

    [HttpPost("registro-natural")]
    public async Task<IActionResult> NaturalExterno([FromBody] RegistroDocumentoRequest request)
    {
        try
        {
            // Intentamos ejecutar el servicio
            var resultado = await service.RegistroPersonaNatural_Home(request);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            // 1. Log del error en el servidor (consola de Visual Studio)
            Console.WriteLine($"Error capturado en el Controlador: {ex.Message}");

            // 2. Devolvemos un BadRequest para que el Frontend lo capture en su bloque 'catch'
            // Esto hará que el error técnico que pusiste en el RAISERROR de SQL aparezca en tu SweetAlert
            return BadRequest(ex.Message);
        }
    }

    //[HttpPost("registro-natural-interno")]
    //public async Task<IActionResult> NaturalInterno(RegistroDocumentoRequest request) => Ok(await service.RegistroTramiteInterno_Home(request));
    //[HttpPost("registro-juridico")]
    //public async Task<IActionResult> Juridico(RegistroJuridicoRequest request) => Ok(await service.RegistrarPersonaJuridicaAsync(request.Documento, request.RucEmpresa, request.RazonSocial));
    //[HttpGet("historial/{personaId:int}")]
    //public async Task<IActionResult> Historial(int personaId) => Ok(await service.ObtenerHistorialTramitesAsync(personaId));
}
