using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

public sealed record LoginRequest(string Documento, string Password);
public sealed record CredencialesRequest(string Documento, string? CodigoOtp, int TipoAccion);

[ApiController]
[Route("api/autenticacion")]
public sealed class AutenticacionController(ILoginService service) : ControllerBase
{
    [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest r) => Ok(await service.ValidarCredencialesAsync(r.Documento, r.Password));
    [HttpPost("credenciales")] public async Task<IActionResult> Credenciales(CredencialesRequest r) => Ok(await service.GestionarCredencialesAsync(r.Documento, r.CodigoOtp, r.TipoAccion));
}
