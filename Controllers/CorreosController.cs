using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

public sealed record OtpRequest(string Correo, string Codigo);
public sealed record CargoRequest(string Correo, string CodigoTramite);
public sealed record ConfirmacionRequest(string Correo, string CodigoTramite, string Asunto);

[ApiController]
[Route("api/correos")]
public sealed class CorreosController(IEmailService service) : ControllerBase
{
    [HttpPost("otp")] public async Task<IActionResult> Otp(OtpRequest r) => Ok(await service.EnviarCodigoOtpAsync(r.Correo, r.Codigo));
    [HttpPost("cargo")] public async Task<IActionResult> Cargo(CargoRequest r) => Ok(await service.EnviarCargoDigitalAsync(r.Correo, r.CodigoTramite));
    [HttpPost("confirmacion")] public async Task<IActionResult> Confirmacion(ConfirmacionRequest r) => Ok(await service.EnviarConfirmacionTramiteAsync(r.Correo, r.CodigoTramite, r.Asunto));
}
