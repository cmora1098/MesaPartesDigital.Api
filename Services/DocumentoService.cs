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
        public async Task<RegistroDocumentoResponse> RegistroPersonaNatural_Home(RegistroDocumentoRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Aquí NO usamos transacción global porque el SP ya tiene una interna
            RegistroDocumentoResponse respuesta = new RegistroDocumentoResponse();
            int? codAsuntoActual = null;

            foreach (var archivo in request.Archivos)
            {
                using var cmd = new SqlCommand("USP_RegistroPersonaNatural", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros de Persona (son los mismos para todos los archivos)
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
                cmd.Parameters.AddWithValue("@iCodAsunto", codAsuntoActual ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@vRutaDoc", archivo.VRutaDoc);
                cmd.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
                cmd.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
                cmd.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
                cmd.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);
                cmd.Parameters.AddWithValue("@vReferencia", archivo.BTipo ? request.VReferencia : "ANEXO");
                cmd.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
                cmd.Parameters.AddWithValue("@btipo", archivo.BTipo);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    if (archivo.BTipo) // Si es el principal, guardamos el Asunto generado
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

        public async Task<RegistroDocumentoResponse> RegistroPersonaJuridica_Home(RegistroDocumentoJuridicoRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            RegistroDocumentoResponse respuesta = new RegistroDocumentoResponse();
            int? codAsuntoActual = null;

            foreach (var archivo in request.Archivos)
            {
                using var cmd = new SqlCommand("USP_RegistroPersonaJuridica", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                // I. Datos Empresa
                cmd.Parameters.AddWithValue("@vRucEmpresa", request.VRucEmpresa);
                cmd.Parameters.AddWithValue("@vRazonSocial", request.VRazonSocial);

                // II. Datos Representante (Usando las propiedades del nuevo request)
                // Nota: He ajustado los nombres para que coincidan con tu nueva clase
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
                cmd.Parameters.AddWithValue("@vReferencia", archivo.BTipo ? request.VReferencia : "ANEXO");
                cmd.Parameters.AddWithValue("@vNroPagFolios", archivo.BTipo ? request.VNroPagFolios : "0");
                cmd.Parameters.AddWithValue("@btipo", archivo.BTipo);
                cmd.Parameters.AddWithValue("@vLink", (object)request.VLink ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    if (archivo.BTipo)
                    {
                        codAsuntoActual = reader.GetInt32(reader.GetOrdinal("iCodAsunto"));
                        respuesta.ICodAsunto = codAsuntoActual.Value;
                        respuesta.VAutoGenerado = reader["vAutoGenerado"].ToString();
                    }
                }
                reader.Close();
            }
            return respuesta;
        }

        //public async Task<RegistroDocumentoResponse> RegistrarPersonaJuridicaAsync(RegistroDocumentoRequest request, string rucEmpresa, string razonSocial)
        //{
        //    using var connection = new SqlConnection(_connectionString);
        //    using var command = new SqlCommand("dbo.USP_RegistroTramiteInternoPersonaJuridica", connection);

        //    command.CommandType = CommandType.StoredProcedure;
        //    command.CommandTimeout = 120;

        //    // 🟢 1. PARÁMETROS DE SESIÓN DEL USUARIO LOGEADO
        //    command.Parameters.AddWithValue("@iCodPerUsuario", request.ICodPer);
        //    command.Parameters.AddWithValue("@vEmailUsuario", request.VEmail);

        //    // 🏢 2. DATOS DE LA EMPRESA
        //    command.Parameters.AddWithValue("@vRucEmpresa", rucEmpresa);
        //    command.Parameters.AddWithValue("@vRazonSocial", razonSocial);

        //    // 📄 3. DATOS DEL DOCUMENTO
        //    command.Parameters.AddWithValue("@iCodAsunto", request.ICodAsunto);
        //    command.Parameters.AddWithValue("@vRutaDoc", (object)request.VRutaDoc ?? DBNull.Value);
        //    command.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
        //    command.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
        //    command.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
        //    command.Parameters.AddWithValue("@vReferencia", request.VReferencia);
        //    command.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);
        //    command.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
        //    command.Parameters.AddWithValue("@btipo", request.BTipo);
        //    command.Parameters.AddWithValue("@vLink", (object)request.VLink ?? DBNull.Value);

        //    await connection.OpenAsync();

        //    using var reader = await command.ExecuteReaderAsync();
        //    if (await reader.ReadAsync())
        //    {
        //        return new RegistroDocumentoResponse
        //        {
        //            ICodDoc = Convert.ToInt32(reader["iCodDoc"]),
        //            ICodAsunto = Convert.ToInt32(reader["iCodAsunto"]),
        //            Status = reader["Status"]?.ToString() ?? "ERROR",
        //            MailSeguimiento = reader["MailSeguimiento"]?.ToString() ?? "",
        //            VAutoGenerado = reader["vAutoGenerado"] != DBNull.Value ? reader["vAutoGenerado"].ToString() : null
        //        };
        //    }

        //    throw new Exception("No se pudo obtener la respuesta del trámite jurídico registrado.");
        //}

        // MANTENIDO: Historial de Trámites x Usuario 
        public async Task<List<TramiteDto>> ObtenerHistorialTramitesAsync(int iCodPer)
        {
            var historial = new List<TramiteDto>();

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
                    historial.Add(new TramiteDto
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
        //public async Task<RegistroDocumentoResponse> RegistroTramiteInterno_Home(RegistroDocumentoRequest request)
        //{
        //    var response = new RegistroDocumentoResponse();

        //    //using (var connection = new SqlConnection(_connectionString))
        //    //{
        //    //    using (var command = new SqlCommand("dbo.USP_RegistroTramiteInternoPersonaNatural", connection))
        //    //    {
        //    //        command.CommandType = CommandType.StoredProcedure;

        //    //        command.Parameters.AddWithValue("@iCodPer", request.ICodPer);
        //    //        command.Parameters.AddWithValue("@vEmail", request.VEmail);

        //    //        command.Parameters.AddWithValue("@iCodAsunto", request.ICodAsunto);
        //    //        command.Parameters.AddWithValue("@vRutaDoc", request.VRutaDoc);
        //    //        command.Parameters.AddWithValue("@iCodTipoDoc", request.ICodTipoDoc);
        //    //        command.Parameters.AddWithValue("@vNroDoc", request.VNroDoc);
        //    //        command.Parameters.AddWithValue("@dFecDoc", request.DFecDoc);
        //    //        command.Parameters.AddWithValue("@vReferencia", request.VReferencia);
        //    //        command.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
        //    //        command.Parameters.AddWithValue("@btipo", request.BTipo);
        //    //        command.Parameters.AddWithValue("@vLink", (object)request.VLink ?? DBNull.Value);
        //    //        command.Parameters.AddWithValue("@vNombreAsunto", request.VNombreAsunto);

        //    //        await connection.OpenAsync();
        //    //        using (var reader = await command.ExecuteReaderAsync())
        //    //        {
        //    //            if (await reader.ReadAsync())
        //    //            {
        //    //                response.ICodDoc = Convert.ToInt32(reader["iCodDoc"]);
        //    //                response.ICodAsunto = Convert.ToInt32(reader["iCodAsunto"]);
        //    //                response.Status = reader["Status"].ToString() ?? "ERROR";
        //    //                response.MailSeguimiento = reader["MailSeguimiento"].ToString() ?? "";
        //    //                response.VAutoGenerado = reader["vAutoGenerado"] != DBNull.Value ? reader["vAutoGenerado"].ToString() : null;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    return response;
        //}

    }
}