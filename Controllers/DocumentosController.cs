using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Models;
using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

    // Inicio Logeado
    [HttpGet("historial/{personaId:int}")]
    public async Task<IActionResult> Historial(int personaId) => Ok(await _service.ObtenerHistorialTramitesAsync(personaId));

    // Editar
    [HttpGet("DatosRegistrados/{id}")]
    public async Task<IActionResult> GetAsunto(int id)
    {
        var asunto = await _service.ObtenerAsuntoParaEdicion(id);

        if (asunto == null)
        {
            return NotFound(new { message = "El trámite no existe o no pudo ser cargado." });
        }

        return Ok(asunto);
    }

    [HttpPut("actualizar-datos-expediente")]
    public async Task<IActionResult> ActualizarDatosExpediente([FromBody] AsuntoEdicionDto request)
    {
        if (request == null || request.iCodAsunto <= 0)
        {
            return BadRequest(new { message = "Los datos enviados no son válidos." });
        }

        try
        {
            // 1. Ejecutas tu actualización en la BD
            await _service.ActualizarDatosExpediente(request);

            // 2. Envías la notificación por correo al usuario
            bool esJuridica = (request.TipoTramite == 1);
            await _emailService.EnviarActualizacionTramiteAsync(
                correoDestino: request.CorreoTramite,
                codigoTramite: request.CodigoTramite ?? request.iCodAsunto.ToString(),
                esPersonaJuridica: esJuridica,
                nuevoRuc: request.RucTramite ?? string.Empty,
                nuevoCorreo: request.CorreoTramite
            );

            return Ok(new { success = true, message = "Datos del expediente actualizados correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al actualizar los datos: " + ex.Message });
        }
    }

    [HttpPut("actualizar-datos-documento")]
    public async Task<IActionResult> ActualizarDatosDocumento([FromBody] AsuntoEdicionDto request)
    {
        if (request == null || request.iCodAsunto <= 0)
        {
            return BadRequest(new { message = "Los datos del documento no son válidos." });
        }

        try
        {
            // 1. Ejecutar la actualización en base de datos
            await _service.ActualizarDatosDocumento(request);

            // 2. Enviar el correo con los cambios específicos detectados
            try
            {
                if (!string.IsNullOrWhiteSpace(request.CorreoTramite))
                {
                    // Si por alguna razón la lista llega vacía, ponemos un respaldo genérico, de lo contrario usamos los cambios reales
                    var listaCambios = (request.CambiosRealizados != null && request.CambiosRealizados.Any())
                        ? request.CambiosRealizados
                        : new List<string> { "La información general del documento ha sido actualizada." };

                    string codigoIdentificador = !string.IsNullOrEmpty(request.CodigoTramite)
                        ? request.CodigoTramite
                        : request.iCodAsunto.ToString();

                    await _emailService.EnviarNotificacionCambiosAsync(
                        correoDestino: request.CorreoTramite,
                        codigoTramite: codigoIdentificador,
                        camposModificados: listaCambios
                    );
                }
            }
            catch (Exception mailEx)
            {
                Console.WriteLine($"[API Error - Correo Documento] No se pudo enviar la notificación: {mailEx.Message}");
            }

            return Ok(new { success = true, message = "Datos del documento actualizados correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Error al actualizar el documento: " + ex.Message });
        }
    }

    [HttpGet("ver-archivo/{id:int}")]
    public async Task<IActionResult> VerArchivoPorId(int id)
    {
        // 1. Obtenemos el registro mediante el servicio existente
        var asunto = await _service.ObtenerAsuntoParaEdicion(id);

        if (asunto == null || string.IsNullOrEmpty(asunto.RutaDocumento))
        {
            return NotFound(new { message = "El trámite o la ruta del documento no existen." });
        }

        // 2. Limpieza de barras por seguridad operativa (local vs producción)
        var rutaFisica = asunto.RutaDocumento.Replace("\\\\", "\\");

        // 3. Validar existencia física del archivo en el servidor
        if (!System.IO.File.Exists(rutaFisica))
        {
            return NotFound(new { message = $"El archivo físico no se encuentra en el servidor: {rutaFisica}" });
        }

        // 4. Retornar el flujo del PDF con su tipo MIME correcto
        var bytes = await System.IO.File.ReadAllBytesAsync(rutaFisica);
        return File(bytes, "application/pdf");
    }

    [HttpPut("actualizar-documento-principal/{idAsunto:int}")]
    public async Task<IActionResult> ActualizarDocumentoPrincipal(int idAsunto, IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { message = "No se ha proporcionado ningún archivo válido." });
        }

        try
        {
            // Convertimos el IFormFile recibido a IBrowserFile o adaptamos el servicio. 
            // Como tu servicio recibe IBrowserFile, creamos un adaptador temporal:
            var browserFileFormAdapter = new FormFileBrowserAdapter(archivo);

            bool resultado = await _service.ActualizarDocumentoPrincipal(idAsunto, browserFileFormAdapter);

            if (!resultado)
            {
                return BadRequest(new { message = "No se pudo actualizar el documento principal." });
            }

            return Ok(new { message = "Documento principal actualizado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
        }
    }

    [HttpGet("listar-anexos/{iCodAsunto}")]
    public async Task<ActionResult<List<AnexoDto>>> ListarAnexos(int iCodAsunto)
    {
        // Se llama al método del servicio en lugar de .Set<>
        var anexos = await _service.ListarAnexosTramite(iCodAsunto);

        return Ok(anexos);
    }

    [HttpGet("ver-archivo-anexo/{iCodDoc}")]
    public async Task<IActionResult> VerArchivoAnexo(int iCodDoc)
    {
        string rutaFisica = await _service.ObtenerRutaPorId(iCodDoc);

        if (string.IsNullOrEmpty(rutaFisica) || !System.IO.File.Exists(rutaFisica))
        {
            return NotFound(new { mensaje = "El archivo físico no se encuentra en el servidor." });
        }

        var bytesArchivo = await System.IO.File.ReadAllBytesAsync(rutaFisica);
        return File(bytesArchivo, "application/pdf");
    }

    [HttpDelete("eliminar-anexo/{iCodDoc:int}")]
    public async Task<IActionResult> EliminarAnexoTramite(int iCodDoc)
    {
        try
        {
            bool eliminado = await _service.EliminarAnexoTramite(iCodDoc);

            if (!eliminado)
            {
                // Ocurrió un error lógico
                return BadRequest(new { mensaje = "No se pudo actualizar el registro." });
            }

            // 🟢 IMPORTANTE: Asegúrate de retornar Ok() para que IsSuccessStatusCode sea true en Blazor
            return Ok(new { success = true, mensaje = "Anexo eliminado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = ex.Message });
        }
    }

    [HttpPost("registrar-anexos/{iCodAsunto:int}")]
    public async Task<IActionResult> RegistrarAnexos(int iCodAsunto, [FromBody] List<ArchivoAdjunto> archivosNuevos)
    {
        try
        {
            bool resultado = await _service.RegistrarNuevosAnexosAsync(iCodAsunto, archivosNuevos);

            if (!resultado)
            {
                return BadRequest(new { mensaje = "El servicio retornó falso al intentar guardar." });
            }

            return Ok(new { success = true, mensaje = "Anexos registrados correctamente." });
        }
        catch (Exception ex)
        {
            // 🟢 AQUÍ DEVOLVEMOS EL ERROR REAL A BLAZOR PARA LEERLO EN PANTALLA
            return StatusCode(500, new { mensaje = ex.Message });
        }
    }

    //[HttpPut("cambiar-estado/{iCodAsunto:int}")]
    //public async Task<IActionResult> CambiarEstadoTramite(int iCodAsunto)
    //{
    //    try
    //    {
    //        bool actualizado = await _service.CambiarEstadoTramiteAsync(iCodAsunto);

    //        if (!actualizado)
    //        {
    //            return BadRequest(new { mensaje = "No se pudo cambiar el estado. Verifique si el trámite existe o si se encuentra en estado 5." });
    //        }

    //        return Ok(new { success = true, mensaje = "El estado del trámite cambió de 5 a 1 correctamente." });
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, new { mensaje = "Error interno del servidor.", detalle = ex.Message });
    //    }
    //}

    [HttpPut("cambiar-estado/{iCodAsunto:int}")]
    public async Task<IActionResult> CambiarEstadoTramite(int iCodAsunto)
    {
        try
        {
            // 1. Cambiar el estado en la base de datos (de 5 a 1)
            bool actualizado = await _service.CambiarEstadoTramiteAsync(iCodAsunto);

            if (!actualizado)
            {
                return BadRequest(new { mensaje = "No se pudo cambiar el estado. Verifique si el trámite existe o si se encuentra en estado 5." });
            }

            // 2. Obtener el correo y el código del trámite utilizando el DTO
            var tramiteInfo = await _service.ObtenerDatosTramiteParaNotificacionAsync(iCodAsunto);

            if (tramiteInfo != null && !string.IsNullOrWhiteSpace(tramiteInfo.CorreoTramite))
            {
                // Definir los cambios o conceptos subsanados para la lista requerida por el correo
                var camposSubsanados = new List<string>
            {
                "Subsanación de observaciones del documento principal",
                "Actualización de requisitos y datos del expediente"
            };

                // 3. Enviar la notificación de que el trámite fue subsanado
                await _emailService.EnviarCorreoSubsanacionAsync(
                    tramiteInfo.CorreoTramite,
                    tramiteInfo.CodigoTramite ?? iCodAsunto.ToString(),
                    camposSubsanados
                );
            }

            return Ok(new { success = true, mensaje = "El estado del trámite cambió de 5 a 1 correctamente y se notificó la subsanación." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno del servidor.", detalle = ex.Message });
        }
    }


    // Tramite Interno
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
