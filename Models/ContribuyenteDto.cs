using System.Text.Json.Serialization;

namespace MesaPartesDigital.Models
{
    public class ContribuyenteDto
    {
        public string ruc { get; set; } = null!;
        public string nombre_razon_social { get; set; } = null!;

        // Propiedad de conveniencia para tu API (JSON)
        [JsonPropertyName("nombreRazonSocial")]
        public string NombreRazonSocial => nombre_razon_social;
    }
}