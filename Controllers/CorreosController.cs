using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MesaPartesDigital.Api.Controllers;

public sealed record OtpRequest(string Correo, string? Codigo);
public sealed record CargoRequest(string Correo, string CodigoTramite);
public sealed record ConfirmacionRequest(string Correo, string CodigoTramite, string Asunto);

[ApiController]
[Route("api/correos")]
public sealed class CorreosController : ControllerBase
{
    private readonly IEmailService _service;
    private readonly IMemoryCache _cache;

    // Usamos un constructor tradicional explícito para evitar conflictos de inyección
    public CorreosController(IEmailService service, IMemoryCache cache)
    {
        _service = service;
        _cache = cache;
    }

    [HttpPost("otp")]
    public async Task<IActionResult> Otp([FromBody] OtpRequest r)
    {
        if (string.IsNullOrEmpty(r.Correo))
            return BadRequest("El correo es obligatorio.");

        // 1. Generamos el código aleatorio en el servidor
        string codigoGenerado = new Random().Next(100000, 999999).ToString();

        // 2. Guardamos en caché por 5 minutos
        _cache.Set(r.Correo, codigoGenerado, TimeSpan.FromMinutes(5));

        // 3. Enviamos
        bool enviado = await _service.EnviarCodigoOtpAsync(r.Correo, codigoGenerado);

        return enviado
            ? Ok(new { mensaje = "Código enviado correctamente." })
            : StatusCode(500, "No se pudo despachar el correo electrónico.");
    }

    [HttpPost("verificar")]
    public async Task<IActionResult> Verificar([FromBody] OtpRequest r)
    {
        // Validamos contra la caché
        if (_cache.TryGetValue(r.Correo, out string? codigoGuardado))
        {
            if (codigoGuardado == r.Codigo)
            {
                _cache.Remove(r.Correo);
                return Ok(new { validado = true });
            }
        }
        return BadRequest("Código incorrecto o expirado.");
    }

    [HttpPost("cargo")]
    public async Task<IActionResult> Cargo([FromBody] CargoRequest r) =>
        Ok(await _service.EnviarCargoDigitalAsync(r.Correo, r.CodigoTramite));

    [HttpPost("confirmacion")]
    public async Task<IActionResult> Confirmacion([FromBody] ConfirmacionRequest r) =>
        Ok(await _service.EnviarConfirmacionTramiteAsync(r.Correo, r.CodigoTramite, r.Asunto));
}