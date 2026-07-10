using MesaPartesDigital.Models;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

public sealed record RegistroJuridicoRequest(RegistroDocumentoRequest Documento, string RucEmpresa, string RazonSocial);

[ApiController]
[Route("api/documentos")]
public sealed class DocumentosController(DocumentoService service) : ControllerBase
{
    [HttpPost("registro-natural-externo")]
    public async Task<IActionResult> NaturalExterno(RegistroDocumentoRequest request) => Ok(await service.RegistroPersonaNatural_Home(request));
    [HttpPost("registro-natural-interno")]
    public async Task<IActionResult> NaturalInterno(RegistroDocumentoRequest request) => Ok(await service.RegistroTramiteInterno_Home(request));
    [HttpPost("registro-juridico")]
    public async Task<IActionResult> Juridico(RegistroJuridicoRequest request) => Ok(await service.RegistrarPersonaJuridicaAsync(request.Documento, request.RucEmpresa, request.RazonSocial));
    [HttpGet("historial/{personaId:int}")]
    public async Task<IActionResult> Historial(int personaId) => Ok(await service.ObtenerHistorialTramitesAsync(personaId));
}
