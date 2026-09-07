using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using MesaPartesDigital.Models;
using MesaPartesDigital.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace MesaPartesDigital.Api.Controllers;


public sealed record LoginRequest(
    string Documento,
    string Password);


public sealed record CredencialesRequest(
    string Documento,
    string? CodigoOtp,
    int TipoAccion);


[ApiController]
[Route("api/autenticacion")]
public sealed class AutenticacionController(
    ILoginService service,
    IConfiguration configuration) : ControllerBase
{

    // ============================================================
    // CONFIGURACIÓN
    // ============================================================

    private const int DuracionTokenMinutos = 15;


    // ============================================================
    // LOGIN
    // ============================================================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        // --------------------------------------------------------
        // Validación básica
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(request.Documento) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                exitoso = false,
                mensaje = "Debe ingresar sus credenciales."
            });
        }


        // --------------------------------------------------------
        // Validar credenciales
        // --------------------------------------------------------

        var resultado =
            await service.ValidarCredencialesAsync(
                request.Documento,
                request.Password);


        if (!resultado.Exitoso)
        {
            return Unauthorized(new
            {
                exitoso = false,
                mensaje = resultado.Mensaje
            });
        }


        // --------------------------------------------------------
        // Generar JWT
        // --------------------------------------------------------

        resultado.Token =
            GenerarToken(resultado);


        return Ok(resultado);
    }


    // ============================================================
    // RENOVAR TOKEN
    // ============================================================

    [Authorize]
    [HttpPost("renovar")]
    public IActionResult Renovar()
    {
        // --------------------------------------------------------
        // Obtener información del usuario desde el JWT actual
        // --------------------------------------------------------

        var iCodPerClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var nombreClaim =
            User.FindFirstValue(
                ClaimTypes.Name);

        var emailClaim =
            User.FindFirstValue(
                ClaimTypes.Email);


        // --------------------------------------------------------
        // Validar identificación
        // --------------------------------------------------------

        if (!int.TryParse(
                iCodPerClaim,
                out var iCodPer))
        {
            return Unauthorized(new
            {
                exitoso = false,
                mensaje =
                    "No se pudo identificar al usuario."
            });
        }


        // --------------------------------------------------------
        // Crear DTO para generar nuevo JWT
        // --------------------------------------------------------

        var usuario =
            new LoginResultDto
            {
                Exitoso = true,

                Mensaje =
                    "Sesión renovada correctamente.",

                ICodPer = iCodPer,

                VNombreCompleto =
                    nombreClaim ?? string.Empty,

                VEmail =
                    emailClaim ?? string.Empty
            };


        // --------------------------------------------------------
        // Generar nuevo JWT
        // --------------------------------------------------------

        usuario.Token =
            GenerarToken(usuario);


        return Ok(usuario);
    }


    // ============================================================
    // GESTIÓN DE CREDENCIALES
    // ============================================================

    [AllowAnonymous]
    [HttpPost("credenciales")]
    public async Task<IActionResult> Credenciales(
        [FromBody] CredencialesRequest request)
    {
        return Ok(
            await service.GestionarCredencialesAsync(
                request.Documento,
                request.CodigoOtp,
                request.TipoAccion));
    }


    // ============================================================
    // GENERAR JWT
    // ============================================================

    private string GenerarToken(
        LoginResultDto usuario)
    {
        // --------------------------------------------------------
        // Obtener clave
        // --------------------------------------------------------

        var jwtKey =
            configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "La configuración 'Jwt:Key' no está configurada.");


        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "La configuración 'Jwt:Key' está vacía.");
        }


        // --------------------------------------------------------
        // Crear clave criptográfica
        // --------------------------------------------------------

        var key =
            Encoding.UTF8.GetBytes(jwtKey);


        var securityKey =
            new SymmetricSecurityKey(key);


        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);


        // --------------------------------------------------------
        // Fechas
        // --------------------------------------------------------

        var ahora =
            DateTime.UtcNow;

        var expiracion =
            ahora.AddMinutes(
                DuracionTokenMinutos);


        // --------------------------------------------------------
        // Claims
        // --------------------------------------------------------

        var claims =
            new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    usuario.ICodPer.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    usuario.VNombreCompleto ?? string.Empty),

                new Claim(
                    ClaimTypes.Email,
                    usuario.VEmail ?? string.Empty)
            };


        // --------------------------------------------------------
        // Descriptor del token
        // --------------------------------------------------------

        var tokenDescriptor =
            new SecurityTokenDescriptor
            {
                Subject =
                    new ClaimsIdentity(claims),

                NotBefore = ahora,

                IssuedAt = ahora,

                Expires = expiracion,

                SigningCredentials = credentials
            };


        // --------------------------------------------------------
        // Crear JWT
        // --------------------------------------------------------

        var tokenHandler =
            new JwtSecurityTokenHandler();


        var token =
            tokenHandler.CreateToken(
                tokenDescriptor);


        return tokenHandler.WriteToken(token);
    }
}