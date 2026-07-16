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
            int? codAsuntoGenerado = null;
            RegistroDocumentoResponseTPN ultimoResultado = null;

            // Iteramos sobre todos los archivos enviados en la lista
            foreach (var archivo in request.Archivos)
            {
                // Creamos una solicitud temporal basada en el archivo actual
                var subRequest = new RegTramitePersNaturalDto
                {
                    ICodPer = request.ICodPer,
                    VEmail = request.VEmail,
                    ICodAsunto = codAsuntoGenerado ?? 0, // Si es el 1ro, 0. Si es 2do+, usa el ID del 1ro
                    VRutaDoc = archivo.VRutaDoc,
                    ICodTipoDoc = request.ICodTipoDoc,
                    VNroDoc = request.VNroDoc,
                    DFecDoc = request.DFecDoc,
                    VNombreAsunto = request.VNombreAsunto,
                    VReferencia = request.VReferencia,
                    VNroPagFolios = request.VNroPagFolios,
                    BTipo = archivo.BTipo // true para el principal, false para anexos
                };

                // Llamamos al servicio para este archivo específico
                var resultado = await _service.RegistroTramiteInterno_PersNatural(subRequest);

                // Guardamos el resultado del primero (el principal) para usar su ID en los siguientes
                if (codAsuntoGenerado == null)
                {
                    codAsuntoGenerado = resultado.ICodAsunto;
                    ultimoResultado = resultado;
                }
            }

            // Envío de correo (solo una vez, después de procesar todo)
            if (ultimoResultado != null)
            {
                _ = _emailService.EnviarConfirmacionTramiteAsync(request.VEmail, ultimoResultado.VAutoGenerado, request.VNombreAsunto);
            }

            return Ok(ultimoResultado);
        }
        catch (Exception ex)
        {
            // Log del error aquí (ej. _logger.LogError(ex, "Error en registro"))
            return BadRequest(new { mensaje = "Error al procesar el trámite", error = ex.Message });
        }
    }

    [HttpPost("registro-tramite-juridica")]
    public async Task<IActionResult> JuridicaInterno([FromBody] RegTramitePersJuridicaDto request)
    {
        try
        {
            // 1. Obtención de datos de sesión desde el token
            int iCodPerUsuario = int.Parse(User.FindFirst("nameid")?.Value ?? "0");
            string vEmailUsuario = User.FindFirst("unique_name")?.Value ?? "";

            int? codAsuntoGenerado = null;
            RegistroDocumentoResponseTPJ ultimoResultado = null;

            // 2. Iteración sobre la lista de archivos del DTO
            foreach (var archivo in request.Archivos)
            {
                // Creamos una solicitud específica para el SP
                // Incluyendo VNombreAsunto para que el SP pueda registrar el asunto correctamente
                var subRequest = new RegTramitePersJuridicaDto
                {
                    VRucEmpresa = request.VRucEmpresa,
                    VRazonSocial = request.VRazonSocial,
                    ICodAsunto = codAsuntoGenerado ?? 0,
                    VRutaDoc = archivo.VRutaDoc,
                    ICodTipoDoc = request.ICodTipoDoc,
                    VNroDoc = request.VNroDoc,
                    DFecDoc = request.DFecDoc,
                    VNombreAsunto = request.VNombreAsunto, // <--- CAMBIO: Agregado
                    VReferencia = request.VReferencia,
                    VNroPagFolios = request.VNroPagFolios,
                    BTipo = archivo.BTipo,
                    VLink = request.VLink
                };

                // 3. Llamada al servicio
                var resultado = await _service.RegistroTramiteInterno_PersJuridica(subRequest, iCodPerUsuario, vEmailUsuario);

                // 4. Capturamos el ID de asunto solo en la primera vuelta
                if (codAsuntoGenerado == null)
                {
                    codAsuntoGenerado = resultado.ICodAsunto;
                    ultimoResultado = resultado;
                }
            }

            // 5. Envío de correo de confirmación
            if (ultimoResultado != null)
            {
                _ = _emailService.EnviarConfirmacionTramiteAsync(vEmailUsuario, ultimoResultado.VAutoGenerado, request.VNombreAsunto);
            }

            return Ok(ultimoResultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = "Error al registrar el trámite de persona jurídica", error = ex.Message });
        }
    }
}
