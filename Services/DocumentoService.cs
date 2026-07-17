using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MesaPartesDigital.Services
{
    public class DocumentoService
    {
        private readonly string _rutaLocalPC = @"C:\MesaDePartesLocal\Archivos";
        private readonly string _connectionString;

        public DocumentoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // Dentro de tu DocumentoService.cs o CatalogosService.cs
        public async Task<List<TipoDocumento>> ObtenerTiposDocumentoActivosAsync()
        {
            var lista = new List<TipoDocumento>();

            using var connection = new SqlConnection(_connectionString);
            // Ajusta el nombre de la tabla si es necesario, asegúrate de que sea accesible
            var query = "SELECT iCodTipoDoc, vNombreTipoDoc, bActivo FROM T_TipoDocumento WHERE bActivo = 1";

            using var command = new SqlCommand(query, connection);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new TipoDocumento
                {
                    iCodTipoDoc = Convert.ToInt32(reader["iCodTipoDoc"]),
                    vNombreTipoDoc = reader["vNombreTipoDoc"].ToString() ?? "",
                    bActivo = Convert.ToBoolean(reader["bActivo"])
                });
            }

            return lista;
        }

        // 🟢 MANTENIDO: Guarda el archivo principal estructurado por Año/Mes
        public async Task<string> GuardarArchivoEnPCAsync(IBrowserFile archivo)
        {
            if (archivo == null) return null;

            try
            {
                string anioActual = DateTime.Now.ToString("yyyy");
                string mesActual = DateTime.Now.ToString("MM");
                string rutaDestinoFinal = Path.Combine(_rutaLocalPC, anioActual, mesActual);

                if (!Directory.Exists(rutaDestinoFinal))
                {
                    Directory.CreateDirectory(rutaDestinoFinal);
                }

                string extension = Path.GetExtension(archivo.Name);
                string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.Name).Replace(" ", "_");
                string nombreUnicoArchivo = $"{Guid.NewGuid()}__{nombreLimpio}{extension}";
                string rutaCompletaPC = Path.Combine(rutaDestinoFinal, nombreUnicoArchivo);

                long maxFileSize = 50 * 1024 * 1024; // 50MB

                using var streamInput = archivo.OpenReadStream(maxFileSize);
                using var streamOutput = File.Create(rutaCompletaPC);

                await streamInput.CopyToAsync(streamOutput);

                return rutaCompletaPC;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar archivo en PC: {ex.Message}");
                throw;
            }
        }

        // 🟢 MANTENIDO: Para los Anexos en Base64 (Alineado a la misma estructura Año/Mes)
        public async Task<string> GuardarArchivoEnPCAsync(string nombreArchivo, string base64Data)
        {
            if (string.IsNullOrEmpty(base64Data)) return null;

            try
            {
                string anioActual = DateTime.Now.ToString("yyyy");
                string mesActual = DateTime.Now.ToString("MM");
                string rutaDestinoFinal = Path.Combine(_rutaLocalPC, anioActual, mesActual);

                if (!Directory.Exists(rutaDestinoFinal))
                {
                    Directory.CreateDirectory(rutaDestinoFinal);
                }

                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                byte[] archivoBytes = Convert.FromBase64String(base64Data);
                string nombreLimpio = nombreArchivo.Replace(" ", "_");
                string nombreUnico = $"{Guid.NewGuid()}_{nombreLimpio}";
                string rutaCompleta = Path.Combine(rutaDestinoFinal, nombreUnico);

                await File.WriteAllBytesAsync(rutaCompleta, archivoBytes);
                return rutaCompleta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DocumentoService] 🚨 Error al decodificar y guardar Base64: {ex.Message}");
                throw;
            }
        }

        // MANTENIDO: Registro Persona Natural Externa
        public async Task<RegistroDocumentoResponse> RegistroPersonaNatural_Home(PersonaNaturalHomeDto request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            RegistroDocumentoResponse respuesta = new RegistroDocumentoResponse();
            int? codAsuntoActual = null;

            // Asegurarse de que los archivos estén ordenados: Principal (false) primero, Anexos (true) después
            var archivosOrdenados = request.Archivos.OrderBy(a => a.BTipo).ToList();

            foreach (var archivo in archivosOrdenados)
            {
                using var cmd = new SqlCommand("USP_RegistroPersonaNatural", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros de Persona
                cmd.Parameters.AddWithValue("@iCodTipoDocPer", request.ICodTipoDocPer);
                cmd.Parameters.AddWithValue("@vDocPer", request.VDocPer);
                cmd.Parameters.AddWithValue("@vNombres", request.VNombres);
                cmd.Parameters.AddWithValue("@vApellidoPaterno", request.VApellidoPaterno);
                cmd.Parameters.AddWithValue("@vApellidoMaterno", request.VApellidoMaterno);
                cmd.Parameters.AddWithValue("@vEmail", request.VEmail);
                cmd.Parameters.AddWithValue("@vTelefono", request.VTelefono);
                cmd.Parameters.AddWithValue("@vDireccion", request.VDireccion);
                cmd.Parameters.AddWithValue("@vCodDistrito", request.VCodDistrito);

                // Parámetros del Documento
                // En la primera vuelta (principal) será null, en las siguientes será el ID generado
                cmd.Parameters.AddWithValue("@iCodAsunto", (object)codAsuntoActual ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vRutaDoc", archivo.VRutaDoc);
                cmd.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
                cmd.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
                cmd.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
                cmd.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);

                // Lógica de Referencia: Si es principal (false) usa la referencia del form, si es anexo usa "ANEXO"
                cmd.Parameters.AddWithValue("@vReferencia", !archivo.BTipo ? request.VReferencia : "ANEXO");

                cmd.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
                cmd.Parameters.AddWithValue("@btipo", archivo.BTipo);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Solo capturamos datos si es el documento principal (BTipo = false)
                    if (!archivo.BTipo)
                    {
                        codAsuntoActual = reader.GetInt32(reader.GetOrdinal("iCodAsunto"));
                        respuesta.ICodAsunto = codAsuntoActual.Value;
                        respuesta.VAutoGenerado = reader["vAutoGenerado"].ToString();
                        respuesta.MailSeguimiento = request.VEmail;
                    }
                }
                reader.Close();
            }

            return respuesta;
        }

        public async Task<RegistroDocumentoResponsePJH> RegistroPersonaJuridica_Home(PersonaJuridicaHomeDto request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            RegistroDocumentoResponsePJH respuesta = new RegistroDocumentoResponsePJH();
            int? codAsuntoActual = null;

            // Ordenar: false (0) va antes que true (1). El Principal será el primero.
            var archivosOrdenados = request.Archivos.OrderBy(a => a.BTipo).ToList();

            foreach (var archivo in archivosOrdenados)
            {
                using var cmd = new SqlCommand("USP_RegistroPersonaJuridica", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // I. Datos Empresa
                cmd.Parameters.AddWithValue("@vRucEmpresa", request.vRucEmpresa);
                cmd.Parameters.AddWithValue("@vRazonSocial", request.VRazonSocial);

                // II. Datos Representante
                cmd.Parameters.AddWithValue("@iCodTipoDocRep", request.ICodTipoDocPer);
                cmd.Parameters.AddWithValue("@vDocRep", request.VDocPer);
                cmd.Parameters.AddWithValue("@vNombresRep", request.VNombres);
                cmd.Parameters.AddWithValue("@vApellidoPaternoRep", request.VApellidoPaterno);
                cmd.Parameters.AddWithValue("@vApellidoMaternoRep", request.VApellidoMaterno);
                cmd.Parameters.AddWithValue("@vEmailRep", request.VEmail);
                cmd.Parameters.AddWithValue("@vTelefonoRep", request.VTelefono);
                cmd.Parameters.AddWithValue("@vDireccionRep", request.VDireccion);
                cmd.Parameters.AddWithValue("@vCodDistritoRep", request.VCodDistrito);

                // III. Datos Documento
                cmd.Parameters.AddWithValue("@iCodAsunto", (object)codAsuntoActual ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@vRutaDoc", archivo.VRutaDoc);
                cmd.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
                cmd.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
                cmd.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
                cmd.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);
                cmd.Parameters.AddWithValue("@vReferencia", !archivo.BTipo ? request.VReferencia : "ANEXO");
                cmd.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
                cmd.Parameters.AddWithValue("@btipo", archivo.BTipo); // 0=Principal, 1=Anexo

                //using var reader = await cmd.ExecuteReaderAsync();
                //if (await reader.ReadAsync())
                //{
                //    // Solo capturamos si es el Principal (0)
                //    if (!archivo.BTipo)
                //    {
                //        codAsuntoActual = reader.GetInt32(reader.GetOrdinal("iCodAsunto"));
                //        respuesta.ICodAsunto = codAsuntoActual.Value;
                //        respuesta.ICodDoc = reader.GetInt32(reader.GetOrdinal("iCodDoc"));
                //        respuesta.VAutoGenerado = reader["vAutoGenerado"].ToString();
                //        respuesta.Status = reader["Status"].ToString();
                //        respuesta.MailSeguimiento = reader["MailSeguimiento"].ToString();
                //    }
                //}
                //reader.Close();

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Solo capturamos datos si es el documento principal (BTipo = false)
                    if (!archivo.BTipo)
                    {
                        codAsuntoActual = reader.GetInt32(reader.GetOrdinal("iCodAsunto"));
                        respuesta.ICodAsunto = codAsuntoActual.Value;
                        respuesta.VAutoGenerado = reader["vAutoGenerado"].ToString();
                        respuesta.MailSeguimiento = request.VEmail;
                    }
                }
                reader.Close();
            }
            return respuesta;
        }

        public async Task<List<HistorialTramiteDto>> ObtenerHistorialTramitesAsync(int iCodPer)
        {
            var historial = new List<HistorialTramiteDto>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("USP_Tramite_ListarHistorialPorUsuario", connection);

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@iCodPer", iCodPer);

                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    historial.Add(new HistorialTramiteDto
                    {
                        Codigo = reader["Codigo"].ToString() ?? string.Empty,
                        Asunto = reader["Asunto"].ToString() ?? string.Empty,
                        Estado = reader["Estado"].ToString() ?? string.Empty,
                        Fecha = reader["Fecha"].ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DocumentoService] 🚨 Error al listar historial: {ex.Message}");
                throw;
            }

            return historial;
        }
         
        //// MANTENIDO: Trámite Interno Persona Natural
        public async Task<RegistroDocumentoResponseTPN> RegistroTramiteInterno_PersNatural(RegTramitePersNaturalDto request)
        {
            var response = new RegistroDocumentoResponseTPN();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("dbo.USP_RegistroTramiteInternoPersonaNatural", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // 1. Parámetros de entrada
                    command.Parameters.AddWithValue("@iCodPer", request.ICodPer);
                    command.Parameters.AddWithValue("@vEmail", request.VEmail);
                    command.Parameters.AddWithValue("@vRutaDoc", request.VRutaDoc);
                    command.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
                    command.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
                    command.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
                    command.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);
                    command.Parameters.AddWithValue("@vReferencia", request.VReferencia);
                    command.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
                    command.Parameters.AddWithValue("@btipo", request.BTipo);
                    command.Parameters.AddWithValue("@vLink", (object)request.VLink ?? DBNull.Value);

                    // 2. Parámetro OUTPUT (ICodAsunto)
                    var pAsunto = new SqlParameter("@iCodAsunto", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.InputOutput,
                        Value = request.ICodAsunto // Si es anexo, aquí vendrá el ID padre
                    };
                    command.Parameters.Add(pAsunto);

                    // 3. Ejecución y lectura de respuesta
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            response.ICodDoc = Convert.ToInt32(reader["iCodDoc"]);
                            response.ICodAsunto = Convert.ToInt32(reader["iCodAsunto"]);
                            response.Status = reader["Status"].ToString();
                            response.MailSeguimiento = reader["MailSeguimiento"].ToString();
                            response.VAutoGenerado = reader["vAutoGenerado"]?.ToString();
                        }
                    }
                }
            }
            return response;
        }

        public async Task<RegistroDocumentoResponseTPJ> RegistroTramiteInterno_PersJuridica(RegTramitePersJuridicaDto request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            RegistroDocumentoResponseTPJ respuesta = new RegistroDocumentoResponseTPJ();
            int? codAsuntoActual = null;

            // Ordenar: 0 (Principal) va antes que 1 (Anexo).
            var archivosOrdenados = request.Archivos.OrderBy(a => a.BTipo).ToList();

            foreach (var archivo in archivosOrdenados)
            {
                using var cmd = new SqlCommand("USP_RegistroTramiteInternoPersonaJuridica", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@iCodPer", request.ICodPer);
                cmd.Parameters.AddWithValue("@vEmail", request.VEmail);
                cmd.Parameters.AddWithValue("@vRucEmpresa", request.VRucEmpresa);
                cmd.Parameters.AddWithValue("@vRazonSocial", request.VRazonSocial);

                // Si es el principal (0), enviamos NULL. Si es anexo (1), enviamos el ID capturado.
                cmd.Parameters.AddWithValue("@iCodAsunto", (object)codAsuntoActual ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@vRutaDoc", archivo.VRutaDoc);
                cmd.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
                cmd.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
                cmd.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
                cmd.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);
                cmd.Parameters.AddWithValue("@vReferencia", !archivo.BTipo ? request.VReferencia : "ANEXO");
                cmd.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
                cmd.Parameters.AddWithValue("@btipo", archivo.BTipo ? 1 : 0);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Solo capturamos el ID si estamos procesando el principal (archivo.BTipo == false)
                    if (!archivo.BTipo)
                    {
                        codAsuntoActual = reader.GetInt32(reader.GetOrdinal("iCodAsunto"));
                        respuesta.ICodAsunto = codAsuntoActual.Value;
                        respuesta.VAutoGenerado = reader["vAutoGenerado"].ToString();
                        respuesta.MailSeguimiento = request.VEmail;
                    }
                }
                reader.Close();
            }
            return respuesta;
        }
    }
}