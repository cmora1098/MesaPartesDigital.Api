using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MesaPartesDigital.Api.Controllers;

public sealed record LoginRequest(string Documento, string Password);
public sealed record CredencialesRequest(string Documento, string? CodigoOtp, int TipoAccion);

[ApiController]
[Route("api/autenticacion")]
public sealed class AutenticacionController(ILoginService service, IConfiguration configuration) : ControllerBase
{
    // [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest r) => Ok(await service.ValidarCredencialesAsync(r.Documento, r.Password));

    //[HttpPost("login")]
    //public async Task<IActionResult> Login(LoginRequest r)
    //{ 
    //    var resultado = await service.ValidarCredencialesAsync(r.Documento, r.Password);

    //    if (!resultado.Exitoso)
    //        return Unauthorized(new { mensaje = resultado.Mensaje });

    //    resultado.Token = GenerarToken(resultado);

    //    return Ok(resultado);
    //}

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var resultado = await service.ValidarCredencialesAsync(r.Documento, r.Password);

        if (!resultado.Exitoso)
            return Unauthorized(new { mensaje = resultado.Mensaje });

        // Generamos el token JWT (si tu frontend lo sigue usando)
        resultado.Token = GenerarToken(resultado);

        // Creamos la identidad para la cookie nativa de ASP.NET Core
        var claims = new List<Claim> {
            new Claim(ClaimTypes.NameIdentifier, resultado.ICodPer.ToString()),
            new Claim(ClaimTypes.Name, resultado.VNombreCompleto),
            new Claim(ClaimTypes.Email, resultado.VEmail ?? string.Empty)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // Emitimos la cookie en el contexto HTTP
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties
        );

        return Ok(resultado);
    }

    private string GenerarToken(LoginResultDto usuario)
    {
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, usuario.ICodPer.ToString()),
            new Claim(ClaimTypes.Name, usuario.VNombreCompleto)
        }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    [HttpPost("credenciales")] public async Task<IActionResult> Credenciales(CredencialesRequest r) => Ok(await service.GestionarCredencialesAsync(r.Documento, r.CodigoOtp, r.TipoAccion));
}
