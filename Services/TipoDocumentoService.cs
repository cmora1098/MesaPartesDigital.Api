using System.Data;
using MesaPartesDigital.Models;
using Microsoft.Data.SqlClient;

namespace MesaPartesDigital.Services
{
    public class TipoDocumentoService
    {
        private readonly string _connectionString;

        public TipoDocumentoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public async Task<List<TipoDocumento>> ObtenerTiposDocumentoAsync()
        {
            var resultado = new List<TipoDocumento>();

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(
                "[dbo].[USP_TIPO_DOCUMENTO_LISTAR]",
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            //comentario
            await connection.OpenAsync();
            await using var reader = await command.ExecuteReaderAsync();

            var ordinalCodigo = reader.GetOrdinal("iCodTipoDoc");
            var ordinalNombre = reader.GetOrdinal("vNombreTipoDoc");

            while (await reader.ReadAsync())
            {
                resultado.Add(new TipoDocumento
                {
                    ICodTipoDoc = reader.GetInt32(ordinalCodigo),
                    VNombreTipoDoc = reader.IsDBNull(ordinalNombre)
                        ? string.Empty
                        : reader.GetString(ordinalNombre)
                });
            }

            return resultado;
        }
    }
}
