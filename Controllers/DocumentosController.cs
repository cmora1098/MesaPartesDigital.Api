using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Models;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

[ApiController]
[Route("api/documentos")]
public sealed class DocumentosController : ControllerBase
{
    private readonly DocumentoService _service;
    private readonly IEmailService _emailService;

    // Inyección de dependencias corregida
    public DocumentosController(DocumentoService service, IEmailService emailService)
    {
        _service = service;
        _emailService = emailService;
    }

    [HttpGet("tipos-documento")]
    public async Task<IActionResult> GetTiposDocumento() => Ok(await _service.ObtenerTiposDocumentoActivosAsync());

    [HttpPost("registro-natural")]
    public async Task<IActionResult> NaturalExterno([FromBody] PersonaNaturalHomeDto request)
    {
        try
        {
            // El servicio ahora procesa todo (Principal + Anexos)
            var resultado = await _service.RegistroPersonaNatural_Home(request);

            // Envío de correo
            _ = _emailService.EnviarConfirmacionTramiteAsync(request.VEmail, resultado.VAutoGenerado, request.VNombreAsunto);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("registro-juridica")]
    public async Task<IActionResult> JuridicaExterno([FromBody] PersonaJuridicaHomeDto request)
    {
        try
        {
            // 1. Llamamos al servicio adaptado para Jurídica
            var resultado = await _service.RegistroPersonaJuridica_Home(request);

            // 2. Envío de correo (el resto de la lógica permanece igual)
            _ = _emailService.EnviarConfirmacionTramiteAsync(request.VEmail, resultado.VAutoGenerado, request.VNombreAsunto);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            // Es una buena práctica registrar el error antes de devolverlo
            return BadRequest(new { message = "Error al procesar el registro: " + ex.Message });
        }
    }

    [HttpGet("historial/{personaId:int}")]
    public async Task<IActionResult> Historial(int personaId) => Ok(await _service.ObtenerHistorialTramitesAsync(personaId));

    //[HttpPost("registro-natural-interno")]
    //public async Task<IActionResult> NaturalInterno(RegistroDocumentoRequest request) => Ok(await service.RegistroTramiteInterno_Home(request));
    //[HttpPost("registro-juridico")]
    //public async Task<IActionResult> Juridico(RegistroJuridicoRequest request) => Ok(await service.RegistrarPersonaJuridicaAsync(request.Documento, request.RucEmpresa, request.RazonSocial));

}
