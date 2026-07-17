using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Models;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;

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

    [HttpPost("registro-tramite-natural")]
    public async Task<IActionResult> NaturalInterno(RegTramitePersNaturalDto request)
    {
        try
        {
            // El servicio registra lo que venga (ya sea principal o anexo)
            var resultado = await _service.RegistroTramiteInterno_PersNatural(request);

            // Si es el documento principal (BTipo == 0 o false), enviamos el correo
            // Nota: Asegúrate de que request.BTipo sea el valor correcto enviado desde el front
            if (request.BTipo == false)
            {
                _ = _emailService.EnviarConfirmacionTramiteAsync(request.VEmail, resultado.VAutoGenerado, request.VNombreAsunto);
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }

    }

    [HttpPost("registro-tramite-juridica")]
    public async Task<IActionResult> JuridicaInterno([FromBody] RegTramitePersJuridicaDto request)
    {
        try
        {
            // 1. Registro a través del servicio
            // El servicio ya se encarga de ordenar y procesar los archivos
            var resultado = await _service.RegistroTramiteInterno_PersJuridica(request);

            // 2. Envío de correo
            // Usamos el resultado devuelto por el servicio. 
            // Si resultado.VAutoGenerado tiene valor, significa que el trámite principal se creó con éxito.
            if (!string.IsNullOrEmpty(resultado.VAutoGenerado))
            {
                _ = _emailService.EnviarConfirmacionTramiteAsync(request.VEmail, resultado.VAutoGenerado, request.VNombreAsunto);
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            // Registro del error (log) y retorno al cliente
            return BadRequest(new { mensaje = "Error al registrar el trámite", detalle = ex.Message });
        }
    }



}
