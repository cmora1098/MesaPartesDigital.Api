using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace MesaPartesDigital.Services
{
    public interface IEmailService
    {
        Task<bool> EnviarCodigoOtpAsync(string correoDestino, string codigoOtp);
        Task<bool> EnviarCargoDigitalAsync(string correoDestino, string codigoTramite); 
        Task<bool> EnviarPasswordSistemaAsync(string correoDestino, string passwordTemporal);
        Task<bool> EnviarConfirmacionTramiteAsync(string correoDestino, string codigoTramite, string asuntoTramite); 
        Task<bool> EnviarActualizacionTramiteAsync(string correoDestino, string codigoTramite, bool esPersonaJuridica, string nuevoRuc, string nuevoCorreo);
        Task<bool> EnviarNotificacionCambiosAsync(string correoDestino, string codigoTramite, List<string> camposModificados);
        Task<bool> EnviarCorreoSubsanacionAsync(string correoDestino, string codigoTramite, List<string> camposModificados);

    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> EnviarCodigoOtpAsync(string correoDestino, string codigoOtp)
        {
            try
            {
                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = "Código de Verificación - Mesa de Partes Digital";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                        <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                            <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                            <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Mesa de Partes Digital</p>
                        </div>
                        <div style='padding: 5%; background-color: #ffffff;'>
                            <p style='color: #333333; font-size: 15px;'>Estimado(a),</p>
                            <p style='color: #555555; font-size: 14px;'>Se ha solicitado un código de verificación para continuar con el registro de su documento en la plataforma.</p>
                            <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                                <span style='font-size: 11px; color: #555555; text-transform: uppercase;'>Código de Verificación</span>
                                <div style='font-size: 30px; font-weight: bold; letter-spacing: 4px; color: #006432;'>{codigoOtp}</div>
                            </div>
                            <div style='font-size: 12px; color: #777777; background-color: #fff8e7; padding: 12px; border-left: 4px solid #f39c12;'>
                                <strong>Importante:</strong> Este código es de un solo uso. Si usted no solicitó este requerimiento, por favor ignore este mensaje.
                            </div>
                            <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                            <p style='font-size: 11px; color: #999999; text-align: center;'>AGRO RURAL - Por favor, no responda a este correo.</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Falló el envío SMTP OTP: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarCargoDigitalAsync(string correoDestino, string codigoTramite)
        {
            try
            {
                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = $"Cargo de Recepción Digital - Trámite {codigoTramite}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                        <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                            <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                            <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Mesa de Partes Digital</p>
                        </div>
                        <div style='padding: 5%; background-color: #ffffff;'>
                            <p style='color: #333333; font-size: 15px;'>Estimado(a),</p>
                            <p style='color: #555555; font-size: 14px;'>Nos complace informarle que su documentación ha sido cargada con éxito en nuestro sistema.</p>
                            <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                                <span style='display: block; font-size: 11px; color: #555555; text-transform: uppercase;'>Código de Trámite Autogenerado</span>
                                <div style='font-size: 26px; font-weight: bold; font-family: monospace; letter-spacing: 2px; color: #006432;'>{codigoTramite}</div>
                            </div>
                            <div style='background-color: #fafafa; border: 1px solid #eeeeee; padding: 15px; border-radius: 6px;'>
                                <table style='width: 100%; font-size: 13px; color: #555555;'>
                                    <tr><td style='padding: 5px 0; font-weight: bold; width: 40%;'>Canal de Atención:</td><td style='padding: 5px 0;'>Mesa de Partes Virtual</td></tr>
                                    <tr><td style='padding: 5px 0; font-weight: bold;'>Mail de Seguimiento:</td><td style='padding: 5px 0; color: #006432; font-weight: bold;'>{correoDestino}</td></tr>
                                    <tr><td style='padding: 5px 0; font-weight: bold;'>Estado del Expediente:</td><td style='padding: 5px 0;'><span style='background-color: #e8f5e9; color: #2e7d32; padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: bold;'>ENVIADO</span></td></tr>
                                </table>
                            </div>
                            <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                            <p style='font-size: 11px; color: #999999; text-align: center;'>AGRO RURAL - Por favor, no responda a este correo.</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Falló el envío SMTP Cargo: {ex.Message}");
                return false;
            }
        }

        // 🛠️ IMPLEMENTACIÓN DEL NUEVO MÉTODO DE ENVÍO DE CONTRASEÑA AUTOGENERADA    
        public async Task<bool> EnviarPasswordSistemaAsync(string correoDestino, string passwordTemporal)
        {
            try
            {
                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = "Cuenta Activada - Credenciales de Acceso Mesa de Partes Digital";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                        <div style='background-color: #006432; padding: 25px 15px; text-align: center;'>
                            <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                            <p style='color: #ffffff; margin: 10px 0 0 0; font-size: 14px; font-weight: bold;'>¡Cuenta Activada Exitosamente!</p>
                        </div>
                        <div style='padding: 5%; background-color: #ffffff;'>
                            <p style='color: #333333; font-size: 15px;'>Estimado(a),</p>
                            <p style='color: #555555; font-size: 14px;'>Su dirección de correo electrónico ha sido verificada de forma correcta. A partir de este momento su cuenta está activa en el sistema.</p>
                            
                            <div style='background-color: #f9fafb; border: 1px solid #e5e7eb; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                                <p style='margin: 5px 0; font-size: 14px; color: #374151;'><strong>Usuario (Correo):</strong> {correoDestino}</p>
                                <p style='margin: 5px 0; font-size: 14px; color: #374151;'><strong>Contraseña Asignada:</strong> <span style='font-family: monospace; background: #e5e7eb; padding: 2px 6px; border-radius: 4px; font-weight: bold;'>{passwordTemporal}</span></p>
                            </div>

                            <div style='font-size: 12px; color: #777777; background-color: #fef3c7; padding: 12px; border-left: 4px solid #d97706;'>
                                <strong>Recomendación de Seguridad:</strong> Le sugerimos cambiar esta contraseña temporal desde su perfil una vez ingrese al sistema por primera vez.
                            </div>
                            <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                            <p style='font-size: 11px; color: #999999; text-align: center;'>Sub Unidad de Tecnologías de la Información - AGRO RURAL</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Falló el envío SMTP Credenciales: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarConfirmacionTramiteAsync(string correoDestino, string codigoTramite, string asuntoTramite)
        {
            try
            {
                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = $"Cargo de Recepción Digital - Trámite {codigoTramite}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                    <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                    <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Mesa de Partes Digital</p>
                </div>
                <div style='padding: 5%; background-color: #ffffff;'>
                    <p style='color: #333333; font-size: 15px;'>Estimado(a),</p>
                    <p style='color: #555555; font-size: 14px;'>Nos complace informarle que su documentación ha sido cargada con éxito en nuestro sistema.</p>
                    <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                        <span style='display: block; font-size: 11px; color: #555555; text-transform: uppercase;'>Código de Trámite Autogenerado</span>
                        <div style='font-size: 26px; font-weight: bold; font-family: monospace; letter-spacing: 2px; color: #006432;'>{codigoTramite}</div>
                    </div>
                    <div style='background-color: #fafafa; border: 1px solid #eeeeee; padding: 15px; border-radius: 6px;'>
                        <table style='width: 100%; font-size: 13px; color: #555555;'>
                            <tr><td style='padding: 5px 0; font-weight: bold; width: 40%;'>Canal de Atención:</td><td style='padding: 5px 0;'>Mesa de Partes Virtual</td></tr>
                            <tr><td style='padding: 5px 0; font-weight: bold;'>Asunto Registrado:</td><td style='padding: 5px 0; text-transform: uppercase;'>{asuntoTramite}</td></tr>
                            <tr><td style='padding: 5px 0; font-weight: bold;'>Mail de Seguimiento:</td><td style='padding: 5px 0; color: #006432; font-weight: bold;'>{correoDestino}</td></tr>
                            <tr><td style='padding: 5px 0; font-weight: bold;'>Estado del Expediente:</td><td style='padding: 5px 0;'><span style='background-color: #e8f5e9; color: #2e7d32; padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: bold;'>ENVIADO</span></td></tr>
                        </table>
                    </div>
                    <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                    <p style='font-size: 11px; color: #999999; text-align: center;'>AGRO RURAL - Por favor, no responda a este correo.</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Error al enviar cargo electrónico: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarActualizacionTramiteAsync(string correoDestino, string codigoTramite, bool esPersonaJuridica, string nuevoRuc, string nuevoCorreo)
        {
            try
            {
                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = $"ACTUALIZACIÓN DE INFORMACIÓN DEL EXPEDIENTE - Trámite {codigoTramite}";

                // Construcción dinámica de los cambios según el tipo de trámite
                string detallePersonaHtml = "";
                if (esPersonaJuridica)
                {
                    detallePersonaHtml = $@"
                <tr>
                    <td style='padding: 5px 0; font-weight: bold;'>RUC:</td>
                    <td style='padding: 5px 0; font-family: monospace; color: #006432; font-weight: bold;'>{nuevoRuc}</td>
                </tr>";
                }

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                    <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                    <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Mesa de Partes Digital</p>
                </div>
                <div style='padding: 5%; background-color: #ffffff;'>
                    <p style='color: #333333; font-size: 15px;'>Estimado(a),</p>
                    <p style='color: #555555; font-size: 14px;'>Le informamos que se han actualizado de manera exitosa los datos de seguimiento de su expediente en nuestro sistema.</p>
                    
                    <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                        <span style='display: block; font-size: 11px; color: #555555; text-transform: uppercase;'>Código de Trámite</span>
                        <div style='font-size: 26px; font-weight: bold; font-family: monospace; letter-spacing: 2px; color: #006432;'>{codigoTramite}</div>
                    </div>

                    <div style='background-color: #fafafa; border: 1px solid #eeeeee; padding: 15px; border-radius: 6px;'>
                        <table style='width: 100%; font-size: 13px; color: #555555;'>
                            {detallePersonaHtml}
                            <tr>
                                <td style='padding: 5px 0; font-weight: bold; width: 40%;'>Correo de Seguimiento:</td>
                                <td style='padding: 5px 0; color: #006432; font-weight: bold;'>{nuevoCorreo}</td>
                            </tr>
                            <tr>
                                <td style='padding: 5px 0; font-weight: bold;'>Estado de la Acción:</td>
                                <td style='padding: 5px 0;'><span style='background-color: #e8f5e9; color: #2e7d32; padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: bold;'>ACTUALIZACIÓN DE INFORMACIÓN DEL EXPEDIENTE</span></td>
                            </tr>
                        </table>
                    </div>

                    <div style='font-size: 12px; color: #777777; background-color: #fff8e7; padding: 12px; border-left: 4px solid #f39c12; margin-top: 20px;'>
                        <strong>Aviso:</strong> Si usted no solicitó esta modificación, por favor comuníquese inmediatamente con la institución.
                    </div>

                    <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                    <p style='font-size: 11px; color: #999999; text-align: center;'>AGRO RURAL - Por favor, no responda a este correo.</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Error al enviar correo de actualización: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarNotificacionCambiosAsync(string correoDestino, string codigoTramite, List<string> camposModificados)
        {
            try
            {
                if (camposModificados == null || !camposModificados.Any() || string.IsNullOrWhiteSpace(correoDestino))
                {
                    return false;
                }

                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));
                mensaje.Subject = $"ACTUALIZACIÓN DE INFORMACIÓN DEL DOCUMENTO - Trámite {codigoTramite}";

                // Construcción dinámica de las filas de la tabla con los cambios detectados
                string filasCambiosHtml = "";
                foreach (var cambio in camposModificados)
                {
                    filasCambiosHtml += $@"
            <tr>
                <td style='padding: 8px 10px; border-bottom: 1px solid #eeeeee; color: #333333; font-size: 13px;'>
                    🔹 {cambio}
                </td>
            </tr>";
                }

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                                <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                                    <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                                        <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                                        <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Mesa de Partes Digital</p>
                                    </div>
                                    <div style='padding: 5%; background-color: #ffffff;'>
                                        <p style='color: #333333; font-size: 15px;'>Estimado(a),</p>
                                        <p style='color: #555555; font-size: 14px;'>Le informamos que se han registrado actualizaciones en la información de su documento asociado al expediente.</p>
            
                                        <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                                            <span style='display: block; font-size: 11px; color: #555555; text-transform: uppercase;'>Código de Trámite</span>
                                            <div style='font-size: 26px; font-weight: bold; font-family: monospace; letter-spacing: 2px; color: #006432;'>{codigoTramite}</div>
                                        </div>

                                        <div style='background-color: #fafafa; border: 1px solid #eeeeee; padding: 15px; border-radius: 6px;'>
                                            <p style='margin: 0 0 10px 0; font-weight: bold; font-size: 13px; color: #006432; text-transform: uppercase;'>Detalle de campos modificados:</p>
                                            <table style='width: 100%; border-collapse: collapse;'>
                                                {filasCambiosHtml}
                                            </table>
                                            <div style='margin-top: 15px; text-align: left;'>
                                                <span style='background-color: #e8f5e9; color: #2e7d32; padding: 4px 10px; border-radius: 10px; font-size: 11px; font-weight: bold; display: inline-block;'>ACTUALIZACIÓN DE INFORMACIÓN DEL DOCUMENTO</span>
                                            </div>
                                        </div>

                                        <div style='font-size: 12px; color: #777777; background-color: #fff8e7; padding: 12px; border-left: 4px solid #f39c12; margin-top: 20px;'>
                                            <strong>Aviso:</strong> Si usted no solicitó esta modificación, por favor comuníquese inmediatamente con la institución.
                                        </div>

                                        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                                        <p style='font-size: 11px; color: #999999; text-align: center;'>AGRO RURAL - Por favor, no responda a este correo.</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Error al enviar correo de notificación de cambios: {ex.Message}");
                return false;
            }
        }
         
        public async Task<bool> EnviarCorreoSubsanacionAsync(string correoDestino, string codigoTramite, List<string> camposModificados)
        {
            try
            {
                if (camposModificados == null || !camposModificados.Any() || string.IsNullOrWhiteSpace(correoDestino))
                {
                    return false;
                }

                var server = _configuration["SmtpSettings:Server"];
                var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
                var senderName = _configuration["SmtpSettings:SenderName"];
                var senderEmail = _configuration["SmtpSettings:SenderEmail"];
                var password = _configuration["SmtpSettings:Password"];

                var mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(senderName, senderEmail));
                mensaje.To.Add(new MailboxAddress("", correoDestino));

                // Asunto directo indicando que el trámite fue subsanado
                mensaje.Subject = $"Trámite {codigoTramite} - Subsanado Exitosamente";

                // Construcción dinámica de las filas de la tabla con los cambios detectados
                string filasCambiosHtml = "";
                foreach (var cambio in camposModificados)
                {
                    filasCambiosHtml += $@"
            <tr>
                <td style='padding: 8px 10px; border-bottom: 1px solid #eeeeee; color: #333333; font-size: 13px;'>
                    ✅ {cambio}
                </td>
            </tr>";
                }

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; width: 100%; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-sizing: border-box;'>
                    <div style='background-color: #35af72; padding: 25px 15px; text-align: center;'>
                        <img src='https://www.gob.pe/rails/active_storage/representations/redirect/eyJfcmFpbHMiOnsiZGF0YSI6NDk2ODQ0LCJwdXIiOiJibG9iX2lkIn19--1fc9a7807cf6c726e857b951ca1a374a8414a140/eyJfcmFpbHMiOnsiZGF0YSI6eyJmb3JtYXQiOiJwbmciLCJyZXNpemVfdG9fbGltaXQiOltudWxsLDQ4XX0sInB1ciI6InZhcmlhdGlvbiJ9fQ==--830247c4bafe7cadca50817d8559bf1a09e3aa28/paga%20gob.pe.png' alt='Logo MIDAGRI - AGRO RURAL' style='max-width: 100%; height: auto; min-height: 40px;' />
                        <p style='color: #a3e2c1; margin: 10px 0 0 0; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Mesa de Partes Digital</p>
                    </div>
                    <div style='padding: 5%; background-color: #ffffff;'>
                        <p style='color: #333333; font-size: 15px;'>Estimado(a) ciudadano(a),</p>
                        <p style='color: #555555; font-size: 14px;'>Le informamos que el trámite indicado ha sido <strong>subsanado</strong> correctamente tras la revisión de las observaciones.</p>
            
                        <div style='background-color: #f4f9f5; border: 1px dashed #006432; padding: 20px 10px; text-align: center; border-radius: 6px; margin: 25px 0;'>
                            <span style='display: block; font-size: 11px; color: #555555; text-transform: uppercase;'>Código de Trámite</span>
                            <div style='font-size: 26px; font-weight: bold; font-family: monospace; letter-spacing: 2px; color: #006432;'>{codigoTramite}</div>
                        </div>

                        <div style='background-color: #fafafa; border: 1px solid #eeeeee; padding: 15px; border-radius: 6px;'>
                            <p style='margin: 0 0 10px 0; font-weight: bold; font-size: 13px; color: #006432; text-transform: uppercase;'>Detalle de lo subsanado / modificado:</p>
                            <table style='width: 100%; border-collapse: collapse;'>
                                {filasCambiosHtml}
                            </table>
                            <div style='margin-top: 15px; text-align: left;'>
                                <span style='background-color: #e8f5e9; color: #2e7d32; padding: 4px 10px; border-radius: 10px; font-size: 11px; font-weight: bold; display: inline-block;'>ESTADO: SUBSANADO (EN PROCESO DE REVISIÓN)</span>
                            </div>
                        </div>

                        <div style='font-size: 12px; color: #777777; background-color: #fff8e7; padding: 12px; border-left: 4px solid #f39c12; margin-top: 20px;'>
                            <strong>Aviso:</strong> Su expediente ha vuelto a la cola para la validación definitiva por parte de la entidad.
                        </div>

                        <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;' />
                        <p style='font-size: 11px; color: #999999; text-align: center;'>AGRO RURAL - Por favor, no responda a este correo.</p>
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
                Console.WriteLine($"[EmailService Error] 🚨 Error al enviar correo de subsanación: {ex.Message}");
                return false;
            }
        }
    }
}