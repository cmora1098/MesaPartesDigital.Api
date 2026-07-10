using System.Data;
using MesaPartesDigital.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace MesaPartesDigital.Services;

public sealed class UbigeoService
{
    private const string CacheDepartamentosKey = "Ubigeo_Departamentos";
    private readonly string _connectionString;
    private readonly IMemoryCache _cache;

    public UbigeoService(
        IConfiguration configuration,
        IMemoryCache cache)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        _cache = cache;
    }

    public async Task<List<Departamento>> ObtenerDepartamentosAsync()
    {
        if (_cache.TryGetValue(CacheDepartamentosKey, out List<Departamento>? departamentos))
        {
            return departamentos ?? [];
        }

        departamentos = [];

        await using var connection = new SqlConnection(_connectionString);
        await using var command = CrearComando(connection, tipoListado: 1, codigoPadre: null);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            departamentos.Add(new Departamento
            {
                vCodDepartamento = reader["vCodDepartamento"]?.ToString() ?? string.Empty,
                vNomDepartamento = reader["vNomDepartamento"]?.ToString() ?? string.Empty,
                dcLongitud = ObtenerDecimal(reader, "dcLongitud"),
                dcLatitud = ObtenerDecimal(reader, "dcLatitud")
            });
        }

        _cache.Set(
            CacheDepartamentosKey,
            departamentos,
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24))
                .SetSlidingExpiration(TimeSpan.FromHours(4)));

        return departamentos;
    }

    public async Task<List<Provincia>> ObtenerProvinciasAsync(string codDepartamento)
    {
        ValidarCodigoUbigeo(codDepartamento, nameof(codDepartamento));
        var cacheKey = $"Ubigeo_Provincias_{codDepartamento}";

        if (_cache.TryGetValue(cacheKey, out List<Provincia>? provincias))
        {
            return provincias ?? [];
        }

        provincias = [];

        await using var connection = new SqlConnection(_connectionString);
        await using var command = CrearComando(connection, tipoListado: 2, codigoPadre: codDepartamento);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            provincias.Add(new Provincia
            {
                vCodDepartamento = reader["vCodDepartamento"]?.ToString() ?? string.Empty,
                vCodProvincia = reader["vCodProvincia"]?.ToString() ?? string.Empty,
                vNomProvincia = reader["vNomProvincia"]?.ToString() ?? string.Empty,
                dcLongitud = ObtenerDecimal(reader, "dcLongitud"),
                dcLatitud = ObtenerDecimal(reader, "dcLatitud")
            });
        }

        _cache.Set(cacheKey, provincias, TimeSpan.FromHours(12));
        return provincias;
    }

    public async Task<List<Distrito>> ObtenerDistritosAsync(string codProvincia)
    {
        ValidarCodigoUbigeo(codProvincia, nameof(codProvincia));
        var cacheKey = $"Ubigeo_Distritos_{codProvincia}";

        if (_cache.TryGetValue(cacheKey, out List<Distrito>? distritos))
        {
            return distritos ?? [];
        }

        distritos = [];

        await using var connection = new SqlConnection(_connectionString);
        await using var command = CrearComando(connection, tipoListado: 3, codigoPadre: codProvincia);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            distritos.Add(new Distrito
            {
                vCodDepartamento = reader["vCodDepartamento"]?.ToString() ?? string.Empty,
                vCodProvincia = reader["vCodProvincia"]?.ToString() ?? string.Empty,
                vCodDistrito = reader["vCodDistrito"]?.ToString() ?? string.Empty,
                vNomDistrito = reader["vNomDistrito"]?.ToString() ?? string.Empty,
                dcLongitud = ObtenerDecimal(reader, "dcLongitud"),
                dcLatitud = ObtenerDecimal(reader, "dcLatitud")
            });
        }

        _cache.Set(cacheKey, distritos, TimeSpan.FromHours(12));
        return distritos;
    }

    private static SqlCommand CrearComando(SqlConnection connection, byte tipoListado, string? codigoPadre)
    {
        var command = new SqlCommand("dbo.USP_UBIGEO_LISTAR", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = 60
        };

        command.Parameters.Add("@TIPO_LISTADO", SqlDbType.TinyInt).Value = tipoListado;
        command.Parameters.Add("@COD_UBIGEO_PADRE", SqlDbType.Char, 6).Value =
            string.IsNullOrWhiteSpace(codigoPadre) ? DBNull.Value : codigoPadre;

        return command;
    }

    private static decimal ObtenerDecimal(SqlDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);
        return reader.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static void ValidarCodigoUbigeo(string codigo, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length != 6 || !codigo.All(char.IsDigit))
        {
            throw new ArgumentException("El código de ubigeo debe contener exactamente 6 dígitos.", nombreParametro);
        }
    }
}
