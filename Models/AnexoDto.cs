namespace MesaPartesDigital.Api.Models
{
    public class AnexoDto
    {
        public int iCodDoc { get; set; }
        public int iCodAsunto { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
