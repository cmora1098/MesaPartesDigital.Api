using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MesaPartesDigital.Api.Models;

namespace MesaPartesDigital.Api.Services
{
    public class PersonaService : IPersonaService
    {
        private readonly string _connectionString;

        public PersonaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<PersonaDto> ObtenerPorIdAsync(int iCodPer)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                iCodPer
            };

            var persona = await connection.QueryFirstOrDefaultAsync<PersonaDto>(
                "USP_Persona_Listar",
                parameters,
                commandType: CommandType.StoredProcedure);

            return persona;
        }

        public async Task<bool> ActualizarAsync(PersonaDto persona)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                iCodPer = persona.ICodPer,
                vNombres = persona.VNombres,
                vApellidoPaterno = persona.VApellidoPaterno,
                vApellidoMaterno = persona.VApellidoMaterno,
                vEmail = persona.VEmail,
                vTelefono = persona.VTelefono,
                vCodDistrito = persona.VCodDistrito,
                vDireccion = persona.VDireccion,
                bCorreoVerificado = persona.BCorreoVerificado
            };

            var filasAfectadas = await connection.ExecuteAsync(
                "USP_Persona_Actualizar",
                parameters,
                commandType: CommandType.StoredProcedure);

            return filasAfectadas > 0;
        }
    }
}  