namespace MesaPartesDigital.Models
{
    public class HistorialTramiteDto
    {
        public int iCodAsunto { get; set; } // Añadir esta propiedad
        public string Codigo { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
    }
}
