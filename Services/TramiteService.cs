using Dapper;
using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace MesaPartesDigital.Api.Services
{
    public class TramiteService
    {
        private readonly string _connectionString;

        public TramiteService(IConfiguration configuration)
        {
            // Lee la cadena de conexión directamente del appsettings.json
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");
        }

        public async Task<IEnumerable<TramiteConsultaDto>> ConsultarCutAsync(int nroTramite, int periodo)
        {
            using IDbConnection dbConnection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@intNroTramite", nroTramite);
            parameters.Add("@intPeriodo", periodo);

            var resultado = await dbConnection.QueryAsync<TramiteConsultaDto>(
                "dbo.USP_CONSULTA_CUT",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60  
            );

            return resultado;
        }
    }
}