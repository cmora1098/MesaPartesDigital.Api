using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



namespace MesaPartesDigital.Api.Controllers;

public sealed record LoginRequest(string Documento, string Password);
public sealed record CredencialesRequest(string Documento, string? CodigoOtp, int TipoAccion);

[ApiController]
[Route("api/autenticacion")]
public sealed class AutenticacionController(ILoginService service, IConfiguration configuration) : ControllerBase
{
    // [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest r) => Ok(await service.ValidarCredencialesAsync(r.Documento, r.Password));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        // 1. Llamamos al servicio
        var resultado = await service.ValidarCredencialesAsync(r.Documento, r.Password);

        if (!resultado.Exitoso)
            return Unauthorized(new { mensaje = resultado.Mensaje });

        // 2. Generamos el token y lo guardamos dentro del objeto resultado
        resultado.Token = GenerarToken(resultado);

        // 3. Devolvemos el DTO completo (o un objeto con los datos que prefieras)
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
