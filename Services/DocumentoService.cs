using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace MesaPartesDigital.Services
{
    public class DocumentoService
    {
        //private readonly string _rutaLocalPC = @"C:\MesaDePartesLocal\Archivos";
        private readonly string _rutaLocalPC = @"C:\MesaDePartesLocal\REGDOC";
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
        //public async Task<string> GuardarArchivoEnPCAsync(IBrowserFile archivo)
        //{
        //    if (archivo == null) return null;

        //    try
        //    {
        //        string anioActual = DateTime.Now.ToString("yyyy");
        //        string mesActual = DateTime.Now.ToString("MM");
        //        string rutaDestinoFinal = Path.Combine(_rutaLocalPC, anioActual, mesActual);

        //        if (!Directory.Exists(rutaDestinoFinal))
        //        {
        //            Directory.CreateDirectory(rutaDestinoFinal);
        //        }

        //        string extension = Path.GetExtension(archivo.Name);
        //        string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.Name).Replace(" ", "_");
        //        string nombreUnicoArchivo = $"{Guid.NewGuid()}__{nombreLimpio}{extension}";
        //        string rutaCompletaPC = Path.Combine(rutaDestinoFinal, nombreUnicoArchivo);

        //        long maxFileSize = 50 * 1024 * 1024; // 50MB

        //        using var streamInput = archivo.OpenReadStream(maxFileSize);
        //        using var streamOutput = File.Create(rutaCompletaPC);

        //        await streamInput.CopyToAsync(streamOutput);

        //        return rutaCompletaPC;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error al guardar archivo en PC: {ex.Message}");
        //        throw;
        //    }
        //}

        // 🟢 MANTENIDO: Para los Anexos en Base64 (Alineado a la misma estructura Año/Mes)
        //public async Task<string> GuardarArchivoEnPCAsync(string nombreArchivo, string base64Data)
        //{
        //    if (string.IsNullOrEmpty(base64Data)) return null;

        //    try
        //    {
        //        string anioActual = DateTime.Now.ToString("yyyy");
        //        string mesActual = DateTime.Now.ToString("MM");
        //        string rutaDestinoFinal = Path.Combine(_rutaLocalPC, anioActual, mesActual);

        //        if (!Directory.Exists(rutaDestinoFinal))
        //        {
        //            Directory.CreateDirectory(rutaDestinoFinal);
        //        }

        //        if (base64Data.Contains(","))
        //        {
        //            base64Data = base64Data.Split(',')[1];
        //        }

        //        byte[] archivoBytes = Convert.FromBase64String(base64Data);
        //        string nombreLimpio = nombreArchivo.Replace(" ", "_");
        //        string nombreUnico = $"{Guid.NewGuid()}_{nombreLimpio}";
        //        string rutaCompleta = Path.Combine(rutaDestinoFinal, nombreUnico);

        //        await File.WriteAllBytesAsync(rutaCompleta, archivoBytes);
        //        return rutaCompleta;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[DocumentoService] 🚨 Error al decodificar y guardar Base64: {ex.Message}");
        //        throw;
        //    }
        //}

        // 🟢 Guardado de archivos (Sin estructura Año/Mes)
        public async Task<string> GuardarArchivoEnPCAsync(IBrowserFile archivo)
        {
            if (archivo == null) return null;

            try
            {
                // Asegurar que la carpeta raíz exista
                if (!Directory.Exists(_rutaLocalPC))
                {
                    Directory.CreateDirectory(_rutaLocalPC);
                }

                string extension = Path.GetExtension(archivo.Name);
                string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.Name).Replace(" ", "_");
                string nombreUnicoArchivo = $"{Guid.NewGuid()}__{nombreLimpio}{extension}";

                // La ruta destino es ahora directamente la raíz REGDOC
                string rutaCompletaPC = Path.Combine(_rutaLocalPC, nombreUnicoArchivo);

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

        // 🟢 Guardado de Base64 (Sin estructura Año/Mes)
        public async Task<string> GuardarArchivoEnPCAsync(string nombreArchivo, string base64Data)
        {
            if (string.IsNullOrEmpty(base64Data)) return null;

            try
            {
                if (!Directory.Exists(_rutaLocalPC))
                {
                    Directory.CreateDirectory(_rutaLocalPC);
                }

                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                byte[] archivoBytes = Convert.FromBase64String(base64Data);
                string nombreLimpio = nombreArchivo.Replace(" ", "_");
                string nombreUnico = $"{Guid.NewGuid()}_{nombreLimpio}";

                // La ruta destino es ahora directamente la raíz REGDOC
                string rutaCompleta = Path.Combine(_rutaLocalPC, nombreUnico);

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
                //cmd.Parameters.AddWithValue("@vReferencia", !archivo.BTipo ? request.VReferencia : "ANEXO");

                cmd.Parameters.AddWithValue("@vReferencia", request.VReferencia);

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

                // Lógica de Referencia: Si es principal (false) usa la referencia del form, si es anexo usa "ANEXO"
                //cmd.Parameters.AddWithValue("@vReferencia", !archivo.BTipo ? request.VReferencia : "ANEXO");

                cmd.Parameters.AddWithValue("@vReferencia", request.VReferencia);

                cmd.Parameters.AddWithValue("@vNroPagFolios", request.VNroPagFolios);
                cmd.Parameters.AddWithValue("@btipo", archivo.BTipo); // 0=Principal, 1=Anexo
                 
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

        // Historial y Editar Tramite
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
                        // Debes agregar esta línea para mapear el ID desde el SP
                        iCodAsunto = Convert.ToInt32(reader["iCodAsunto"]),

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

        public async Task<AsuntoEdicionDto?> ObtenerAsuntoParaEdicion(int iCodAsunto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("[dbo].[USP_ObtenerDetalleTramite]", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Agregar parámetro de forma segura
                command.Parameters.Add("@iCodAsunto", SqlDbType.Int).Value = iCodAsunto;

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new AsuntoEdicionDto
                        {
                            iCodAsunto = reader.GetInt32(reader.GetOrdinal("iCodAsunto")),
                            CodigoTramite = reader["CodigoTramite"].ToString() ?? "",
                            AsuntoTramite = reader["AsuntoTramite"].ToString() ?? "",
                            EstadoTramite = reader["EstadoTramite"].ToString() ?? "",
                            CUTTramite = reader["CUTTramite"] != DBNull.Value ? reader["CUTTramite"].ToString() : null,
                            CodigoDependencia = reader["CodigoDependencia"] != DBNull.Value ? (int?)reader["CodigoDependencia"] : null,
                            NombreDependencia = reader["NombreDependencia"].ToString() ?? "",
                            Fecha = reader["Fecha"] != DBNull.Value ? (DateTime)reader["Fecha"] : DateTime.MinValue,
                            CorreoTramite = reader["CorreoTramite"].ToString() ?? "",

                            iCodDoc = reader.GetInt32(reader.GetOrdinal("iCodDoc")),
                            iCodTipoDoc = reader.GetInt32(reader.GetOrdinal("iCodTipoDoc")),
                            NroDocumento = reader["NroDocumento"].ToString() ?? "",
                            FoliosDocumento = reader["FoliosDocumento"].ToString() ?? "",
                            FechaDocumento = reader["FechaDocumento"] != DBNull.Value ? (DateTime)reader["FechaDocumento"] : DateTime.MinValue,
                            RefDocumento = reader["RefDocumento"].ToString() ?? "",
                            RutaDocumento = reader["RutaDocumento"].ToString() ?? "",

                            TipoTramite = reader.GetInt32(reader.GetOrdinal("TipoTramite")),
                            RucTramite = reader["RucTramite"] != DBNull.Value ? reader["RucTramite"].ToString() : null
                             
                        };
                    }
                }
            }
            return null; // Si no encuentra nada
        }

        public async Task<bool> ActualizarDatosExpediente(AsuntoEdicionDto dto)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("USP_ActualizarDatosTramite", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@iCodAsunto", dto.iCodAsunto);
            cmd.Parameters.AddWithValue("@TipoTramite", dto.TipoTramite);
            cmd.Parameters.AddWithValue("@CorreoTramite", (object)dto.CorreoTramite ?? DBNull.Value);

            // Si es Persona Jurídica (1), enviamos el RUC; de lo contrario, NULL
            if (dto.TipoTramite == 1)
            {
                cmd.Parameters.AddWithValue("@RucTramite", (object)dto.RucTramite ?? DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@RucTramite", DBNull.Value);
            }

            await connection.OpenAsync();
            int filasAfectadas = await cmd.ExecuteNonQueryAsync();

            return filasAfectadas > 0;
        }

        public async Task<bool> ActualizarDatosDocumento(AsuntoEdicionDto dto)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("USP_ActualizarDatosDocumentoExpediente", connection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@iCodAsunto", dto.iCodAsunto);
            cmd.Parameters.AddWithValue("@vNombreAsunto", (object)dto.AsuntoTramite ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@iCodTipoDoc", dto.iCodTipoDoc);
            cmd.Parameters.AddWithValue("@vNroDoc", (object)dto.NroDocumento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dFecDoc", dto.FechaDocumento == default ? DBNull.Value : dto.FechaDocumento);
            cmd.Parameters.AddWithValue("@vReferencia", (object)dto.RefDocumento ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@vNroPagFolios", (object)dto.FoliosDocumento ?? DBNull.Value);

            await connection.OpenAsync();
            int filasAfectadas = await cmd.ExecuteNonQueryAsync();

            return filasAfectadas > 0;
        }

        public async Task<bool> ActualizarDocumentoPrincipal(int iCodAsunto, IBrowserFile archivoNuevo)
        {
            if (archivoNuevo == null)
                throw new ArgumentException("El archivo proporcionado es nulo.");

            // 1. Guardar el archivo nuevo en la PC / Servidor
            string nuevaRutaFisica = await GuardarArchivoEnPCAsync(archivoNuevo);

            if (string.IsNullOrEmpty(nuevaRutaFisica))
                throw new Exception("Fallo al guardar el archivo físico en la ruta especificada.");

            // 2. Ejecutar el Stored Procedure en la base de datos
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("USP_ActualizarDocumentoPrincipal", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@iCodAsunto", iCodAsunto);
            command.Parameters.AddWithValue("@vNuevaRutaDoc", nuevaRutaFisica);

            await connection.OpenAsync();

            // Se ejecuta el comando. Si la base de datos ejecuta el SP correctamente sin arrojar excepción SQL, avanzará.
            await command.ExecuteNonQueryAsync();

            return true;
        }

        public async Task<List<AnexoDto>> ListarAnexosTramite(int iCodAsunto)
        {
            var lista = new List<AnexoDto>();

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("USP_ListarAnexosTramite", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@iCodAsunto", iCodAsunto);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                lista.Add(new AnexoDto
                {
                    iCodDoc = reader.GetInt32(reader.GetOrdinal("iCodDoc")),
                    iCodAsunto = reader.GetInt32(reader.GetOrdinal("iCodAsunto")),
                    Nombre = reader["Nombre"] as string ?? string.Empty,
                    Ruta = reader["Ruta"] as string ?? string.Empty,
                    Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                    Descripcion = reader["Descripcion"] as string ?? string.Empty
                });
            }

            return lista;
        }

        public async Task<string> ObtenerRutaPorId(int iCodDoc)
        {
            string ruta = string.Empty;

            using var connection = new SqlConnection(_connectionString);
            // Usamos el iCodDoc para traer la ruta exacta de la base de datos
            using var command = new SqlCommand("SELECT vRutaDoc FROM [BD_RCPDOC].[dbo].[T_Documento] WHERE iCodDoc = @iCodDoc", connection);

            command.Parameters.AddWithValue("@iCodDoc", iCodDoc);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            if (result != null && result != DBNull.Value)
            {
                ruta = result.ToString() ?? string.Empty;
            }

            return ruta;
        }

        //public async Task<bool> EliminarAnexoTramite(int iCodDoc)
        //{
        //    try
        //    {
        //        // Opcional: Si deseas también borrar el archivo físico del disco duro al hacer el borrado lógico:
        //        string rutaFisica = string.Empty;

        //        using (var conn = new SqlConnection(_connectionString))
        //        {
        //            // Primero obtenemos la ruta por si el requerimiento exige borrar el archivo de REGDOC
        //            using var cmdRuta = new SqlCommand("SELECT vRutaDoc FROM [BD_RCPDOC].[dbo].[T_Documento] WHERE iCodDoc = @iCodDoc", conn);
        //            cmdRuta.Parameters.AddWithValue("@iCodDoc", iCodDoc);
        //            await conn.OpenAsync();
        //            var res = await cmdRuta.ExecuteScalarAsync();
        //            if (res != null && res != DBNull.Value) rutaFisica = res.ToString() ?? string.Empty;
        //        }

        //        // Ejecutamos tu Stored Procedure para actualizar bActivo = 0
        //        using (var connection = new SqlConnection(_connectionString))
        //        {
        //            using var command = new SqlCommand("USP_EliminarAnexoTramite", connection)
        //            {
        //                CommandType = CommandType.StoredProcedure
        //            };

        //            command.Parameters.AddWithValue("@iCodDoc", iCodDoc);

        //            await connection.OpenAsync();
        //            int filasAfectadas = await command.ExecuteNonQueryAsync();

        //            if (filasAfectadas > 0)
        //            {
        //                // (Opcional) Borrar archivo físico si existe
        //                // if (!string.IsNullOrEmpty(rutaFisica) && File.Exists(rutaFisica))
        //                // {
        //                //     try { File.Delete(rutaFisica); } catch { }
        //                // }
        //                return true;
        //            }
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error al eliminar anexo: {ex.Message}");
        //        return false;
        //    }
        //}
        public async Task<bool> EliminarAnexoTramite(int iCodDoc)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("USP_EliminarAnexoTramite", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@iCodDoc", iCodDoc);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                // Retornamos true porque el SP se ejecutó exitosamente
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar anexo: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RegistrarNuevosAnexosAsync(int iCodAsunto, List<ArchivoAdjunto> archivosNuevos)
        {
            try
            {
                if (archivosNuevos == null || !archivosNuevos.Any()) return true;

                foreach (var archivo in archivosNuevos)
                {
                    // 1. Asegurar que la carpeta raíz exista
                    if (!Directory.Exists(_rutaLocalPC))
                    {
                        Directory.CreateDirectory(_rutaLocalPC);
                    }

                    // 2. Generar nombre único y ruta completa en REGDOC
                    string extension = Path.GetExtension(archivo.Nombre);
                    string nombreLimpio = Path.GetFileNameWithoutExtension(archivo.Nombre).Replace(" ", "_");
                    string nombreUnicoArchivo = $"{Guid.NewGuid()}__{nombreLimpio}{extension}";
                    string rutaCompletaPC = Path.Combine(_rutaLocalPC, nombreUnicoArchivo);

                    // 3. Guardar el archivo físicamente en el disco usando los bytes
                    await File.WriteAllBytesAsync(rutaCompletaPC, archivo.Contenido);

                    // 4. Ejecutar el Stored Procedure inteligente que clona los datos del documento principal
                    using var connection = new SqlConnection(_connectionString);
                    using var command = new SqlCommand("USP_RegistrarAnexoTramite", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@iCodAsunto", iCodAsunto);
                    command.Parameters.AddWithValue("@vRutaDoc", rutaCompletaPC);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar nuevos anexos: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CambiarEstadoTramiteAsync(int iCodAsunto)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var command = new SqlCommand("USP_CambiarEstadoTramite", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@iCodAsunto", iCodAsunto);

                await connection.OpenAsync();

                // Ejecutamos y leemos el resultado escalar (1 o 0) que retorna el SP
                var resultado = await command.ExecuteScalarAsync();

                if (resultado != null && Convert.ToInt32(resultado) == 1)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cambiar el estado del trámite: {ex.Message}");
                return false;
            }
        }

        public async Task<AsuntoEdicionDto> ObtenerDatosTramiteParaNotificacionAsync(int iCodAsunto)
        {
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT vMailSeguimiento AS CorreoTramite, vAutoGenerado AS CodigoTramite FROM [BD_RCPDOC].[dbo].[T_Asunto] WHERE iCodAsunto = @iCodAsunto";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@iCodAsunto", iCodAsunto);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AsuntoEdicionDto
                {
                    iCodAsunto = iCodAsunto,
                    CorreoTramite = reader["CorreoTramite"]?.ToString(),
                    CodigoTramite = reader["CodigoTramite"]?.ToString()
                };
            }
            return null;
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