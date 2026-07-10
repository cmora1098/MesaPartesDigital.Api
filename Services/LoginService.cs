using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MesaPartesDigital.Services
{
    public class LoginService : ILoginService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public LoginService(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public async Task<LoginResultDto> ValidarCredencialesAsync(string email, string password)
        {
            var resultado = new LoginResultDto { Exitoso = false, Mensaje = "Credenciales incorrectas" };
            string passwordHash = ConvertirASha256(password);

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.USP_Persona_ValidarLogin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@vEmail", SqlDbType.VarChar, 250).Value = email;
                        cmd.Parameters.Add("@vPassword", SqlDbType.VarChar, 64).Value = passwordHash;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // 1. Validamos siempre el estado de éxito
                                resultado.Exitoso = Convert.ToInt32(reader["Exitoso"]) == 1;
                                resultado.Mensaje = reader["Mensaje"]?.ToString() ?? "Error desconocido";

                                // 2. Solo intentamos leer datos si realmente fue exitoso
                                if (resultado.Exitoso)
                                {
                                    // Usamos un método de extensión o validación simple para evitar errores de columnas nulas
                                    resultado.ICodPer = reader["iCodPer"] != DBNull.Value ? Convert.ToInt32(reader["iCodPer"]) : 0;
                                    resultado.VNombreCompleto = reader["vNombreCompleto"]?.ToString() ?? "Usuario";
                                    resultado.VEmail = reader["vEmail"]?.ToString() ?? email;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error técnico: {ex.Message}";
                // Opcional: _logger.LogError(ex, "Error en login");
            }

            return resultado;
        }

        public async Task<GestionCredencialesResultDto> GestionarCredencialesAsync(string email, string? codigoOtp, int tipoAccion)
        {
            var resultado = new GestionCredencialesResultDto();

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.USP_Persona_GestionarCredenciales", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@vEmail", SqlDbType.VarChar, 250).Value = email;
                        cmd.Parameters.Add("@vCodigoOTP", SqlDbType.VarChar, 10).Value = (object?)codigoOtp ?? DBNull.Value;
                        cmd.Parameters.Add("@iTipoAccion", SqlDbType.Int).Value = tipoAccion;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Captura segura de los alias exactos devueltos por el STORE PROCEDURE
                                resultado.Exitoso = Convert.ToInt32(reader["Exitoso"]) == 1;
                                resultado.Mensaje = reader["Mensaje"]?.ToString() ?? "Operación procesada.";
                                resultado.PasswordGenerado = reader["PasswordGenerado"] != DBNull.Value ? reader["PasswordGenerado"]?.ToString() : null;
                            }
                            else
                            {
                                resultado.Exitoso = false;
                                resultado.Mensaje = "No se recibió respuesta estructurada desde la base de datos.";
                            }
                        }
                    }
                }

                // ✉️ Despacho de Correo Automatizado con MailKit si la operación fue exitosa
                if (resultado.Exitoso && !string.IsNullOrEmpty(resultado.PasswordGenerado))
                {
                    string asunto = tipoAccion == 1 ? "Cuenta Activada - Mesa de Partes Digital" : "Restablecimiento de Contraseña - Mesa de Partes Digital";
                    string descripcionAccion = tipoAccion == 1
                        ? "Se ha completado la validación de su cuenta. A continuación, le brindamos su contraseña autogenerada para el acceso al sistema:"
                        : "Se ha solicitado un restablecimiento de sus credenciales. A continuación, le brindamos su nueva contraseña de acceso:";

                    await EnviarCorreoClaveAsync(email, asunto, descripcionAccion, resultado.PasswordGenerado);
                }
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                // 🔍 Captura el mensaje técnico real de la excepción (evita mapeos nulos o literales)
                resultado.Mensaje = $"Error al gestionar las credenciales: {ex.Message}";
            }

            return resultado;
        }

        private async Task<bool> EnviarCorreoClaveAsync(string correoDestino, string asunto, string descripcion, string passwordGenerado)
        {
            try
            {
                var server = _configuration["SmtpSettings:Server"] ?? string.Empty;
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"] ?? string.Empty;
                var senderEmail = _configuration["SmtpSettings:SenderEmail"] ?? string.Empty;
                var password = _configuration["SmtpSettings:Password"] ?? string.Empty;

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = asunto;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); box-sizing: border-box;'>
                        <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                            <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; display: inline-block; min-height: 40px;' />
                            <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold; letter-spacing: 1px;'>Mesa de Partes Digital</p>
                        </div>
                        <div style='padding: 5%; background-color: #ffffff; box-sizing: border-box;'>
                            <p style='color: #333333; font-size: 15px; line-height: 1.5; margin-top: 0;'>Estimado(a),</p>
                            <p style='color: #555555; font-size: 14px; line-height: 1.6;'>{descripcion}</p>
                            <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                                <span style='display: block; font-size: 11px; color: #555555; text-transform: uppercase; margin-bottom: 8px; font-weight: bold; letter-spacing: 1px;'>Nueva Contraseña de Acceso</span>
                                <div style='font-size: 26px; font-weight: bold; letter-spacing: 2px; color: #006432; word-break: break-all; font-family: monospace;'>{passwordGenerado}</div>
                            </div>
                            <div style='font-size: 12px; color: #777777; line-height: 1.4; background-color: #fff8e7; padding: 12px; border-left: 4px solid #f39c12; border-radius: 4px; box-sizing: border-box;'>
                                <strong>Importante:</strong> Le sugerimos cambiar esta contraseña temporal desde la configuración de su perfil una vez que logre ingresar al sistema.
                            </div>
                            <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                            <p style='font-size: 11px; color: #999999; text-align: center; line-height: 1.5; margin: 0;'>Programa de Desarrollo Productivo Agrario Rural - AGRO RURAL<br><span style='color: #ba2525; font-weight: bold; display: block; margin-top: 5px;'>Por favor, no responda a este correo automático.</span></p>
                        </div>
                    </div>"
                };
                mensaje.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(server, port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(mensaje);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Email Error] 🚨 Error al despachar credenciales por SMTP: {ex.Message}");
                return false;
            }
        }

        // 🛠️ Función que iguala al LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', ...), 2)) de SQL
        private string ConvertirASha256(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano)) return string.Empty;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}