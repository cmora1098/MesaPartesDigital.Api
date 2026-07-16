using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

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

        //public async Task<LoginResultDto> ValidarCredencialesAsync(string documento, string password)
        //{
        //    var resultado = new LoginResultDto { Exitoso = false, Mensaje = "Credenciales incorrectas" };
        //    string passwordHash = ConvertirASha256(password);

        //    try
        //    {
        //        using (var conn = new SqlConnection(_connectionString))
        //        {
        //            await conn.OpenAsync();

        //            using (var cmd = new SqlCommand("dbo.USP_Persona_ValidarLogin", conn))
        //            {
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                // CAMBIO: Asegúrate de que el SP acepte @vDocPer en lugar de @vEmail
        //                cmd.Parameters.Add("@vDocPer", SqlDbType.VarChar, 20).Value = documento;
        //                cmd.Parameters.Add("@vPassword", SqlDbType.VarChar, 64).Value = passwordHash;

        //                using (var reader = await cmd.ExecuteReaderAsync())
        //                {
        //                    if (await reader.ReadAsync())
        //                    {
        //                        resultado.Exitoso = Convert.ToInt32(reader["Exitoso"]) == 1;
        //                        resultado.Mensaje = reader["Mensaje"]?.ToString() ?? "Error desconocido";

        //                        if (resultado.Exitoso)
        //                        {
        //                            resultado.ICodPer = reader["iCodPer"] != DBNull.Value ? Convert.ToInt32(reader["iCodPer"]) : 0;
        //                            resultado.VNombreCompleto = reader["vNombreCompleto"]?.ToString() ?? "Usuario";
        //                            // Opcional: Si el SP devuelve el DNI, guárdalo también
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resultado.Exitoso = false;
        //        resultado.Mensaje = $"Error técnico: {ex.Message}";
        //    }
        //    return resultado;
        //}

        public async Task<LoginResultDto> ValidarCredencialesAsync(string documento, string password)
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
                        cmd.Parameters.Add("@vDocPer", SqlDbType.VarChar, 20).Value = documento;
                        cmd.Parameters.Add("@vPassword", SqlDbType.VarChar, 64).Value = passwordHash;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (Convert.ToInt32(reader["Exitoso"]) == 1)
                                {                                  
                                    resultado.Exitoso = true;
                                    resultado.Mensaje = "Acceso concedido";
                                    resultado.ICodPer = Convert.ToInt32(reader["iCodPer"]);
                                    resultado.VNombreCompleto = reader["vNombreCompleto"]?.ToString() ?? "Usuario";
                                    resultado.VEmail = reader["vEmail"]?.ToString() ?? ""; 

                                    // GENERACIÓN DEL TOKEN
                                    var info = new UsuarioInfo
                                    {
                                        ICodPer = resultado.ICodPer,
                                        VNombreCompleto = resultado.VNombreCompleto,
                                        Documento = documento
                                    };

                                    resultado.Token = GenerarTokenJwt(info);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Mensaje = $"Error técnico: {ex.Message}";
            }
            return resultado;
        }

        private string GenerarTokenJwt(UsuarioInfo usuario)
        {
            var secretKey = _configuration["Jwt:Key"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, usuario.ICodPer.ToString()),
        new Claim(ClaimTypes.Name, usuario.VNombreCompleto),
        new Claim("documento", usuario.Documento)
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
        }

        public class UsuarioInfo
        {
            public int ICodPer { get; set; }
            public string VNombreCompleto { get; set; } = string.Empty;
            public string Documento { get; set; } = string.Empty;
        }


        public async Task<GestionCredencialesResultDto> GestionarCredencialesAsync(string dni, string? codigoOtp, int tipoAccion)
        {
            var resultado = new GestionCredencialesResultDto();
            string? correoUsuario = null;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // 1. Ejecutar el SP de Gestión
                    using (var cmd = new SqlCommand("dbo.USP_Persona_GestionarCredenciales", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@vDocPer", SqlDbType.VarChar, 20).Value = dni;
                        cmd.Parameters.Add("@vCodigoOTP", SqlDbType.VarChar, 10).Value = (object?)codigoOtp ?? DBNull.Value;
                        cmd.Parameters.Add("@iTipoAccion", SqlDbType.Int).Value = tipoAccion;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                resultado.Exitoso = Convert.ToInt32(reader["Exitoso"]) == 1;
                                resultado.Mensaje = reader["Mensaje"]?.ToString() ?? "Operación procesada.";
                                resultado.PasswordGenerado = reader["PasswordGenerado"] != DBNull.Value ? reader["PasswordGenerado"]?.ToString() : null;
                            }
                            else
                            {
                                resultado.Exitoso = false;
                                resultado.Mensaje = "No se recibió respuesta estructurada de la base de datos.";
                                return resultado;
                            }
                        }
                    }

                    // 2. Si la operación fue exitosa (Generar clave o Verificar), recuperamos el correo
                    if (resultado.Exitoso)
                    {
                        using (var cmdEmail = new SqlCommand("SELECT vEmail FROM [dbo].[T_Persona] WHERE vDocPer = @vDocPer", conn))
                        {
                            cmdEmail.Parameters.Add("@vDocPer", SqlDbType.VarChar, 20).Value = dni;
                            var emailResult = await cmdEmail.ExecuteScalarAsync();
                            correoUsuario = emailResult?.ToString();
                            resultado.EmailDestino = correoUsuario; // Se asigna para el frontend
                        }
                    }
                }

                // 3. Si se generó clave o se activó, enviamos correo
                if (resultado.Exitoso && !string.IsNullOrEmpty(resultado.PasswordGenerado) && !string.IsNullOrEmpty(correoUsuario))
                {
                    string asunto = tipoAccion == 1 ? "Cuenta Activada - Mesa de Partes Digital" : "Restablecimiento de Contraseña - Mesa de Partes Digital";
                    string descripcion = tipoAccion == 1
                        ? "Se ha completado la validación de su cuenta. Aquí tiene su contraseña:"
                        : "Se ha solicitado un restablecimiento de sus credenciales. A continuación, le brindamos su nueva contraseña de acceso:";

                    bool correoEnviado = await EnviarCorreoClaveAsync(correoUsuario, asunto, descripcion, resultado.PasswordGenerado);

                    if (!correoEnviado)
                    {
                        resultado.Mensaje = "La credencial fue generada, pero hubo un error al enviar el correo.";
                    }
                }
            }
            catch (Exception ex)
            {
                resultado.Exitoso = false;
                resultado.Mensaje = $"Error crítico: {ex.Message}";
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


    }
}