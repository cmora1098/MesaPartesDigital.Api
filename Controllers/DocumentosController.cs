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
    public async Task<IActionResult> NaturalExterno([FromBody] RegistroDocumentoRequest request)
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

    [HttpPost("registrar-juridica")]
    public async Task<IActionResult> RegistrarJuridica([FromBody] RegistroDocumentoJuridicoRequest request)
    {
        try
        { 
            var resultado = await _service.RegistroPersonaJuridica_Home(request);
             
            _ = _emailService.EnviarConfirmacionTramiteAsync(request.VEmail, resultado.VAutoGenerado, request.VNombreAsunto);

            // 3. Asignar el correo al objeto de respuesta para que el frontend lo muestre
            resultado.MailSeguimiento = request.VEmail;

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    //[HttpPost("registro-natural-interno")]
    //public async Task<IActionResult> NaturalInterno(RegistroDocumentoRequest request) => Ok(await service.RegistroTramiteInterno_Home(request));
    //[HttpPost("registro-juridico")]
    //public async Task<IActionResult> Juridico(RegistroJuridicoRequest request) => Ok(await service.RegistrarPersonaJuridicaAsync(request.Documento, request.RucEmpresa, request.RazonSocial));
    //[HttpGet("historial/{personaId:int}")]
    //public async Task<IActionResult> Historial(int personaId) => Ok(await service.ObtenerHistorialTramitesAsync(personaId));
}
