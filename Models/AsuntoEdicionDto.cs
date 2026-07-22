namespace MesaPartesDigital.Api.Models
{
    public class AsuntoEdicionDto
    {
        public int iCodAsunto { get; set; }
        public string? CodigoTramite { get; set; }
        public string? AsuntoTramite { get; set; }
        public string? EstadoTramite { get; set; }
        public string? CUTTramite { get; set; }
        public int? CodigoDependencia { get; set; }
        public string? NombreDependencia { get; set; }
        public DateTime Fecha { get; set; }
        public string? CorreoTramite { get; set; }

        public int iCodDoc { get; set; }
        public int iCodTipoDoc { get; set; }

        public string? NroDocumento { get; set; }
        public string? FoliosDocumento { get; set; }
        public DateTime FechaDocumento { get; set; }
        public string? RefDocumento { get; set; }
        public string? RutaDocumento { get; set; }

        public int TipoTramite { get; set; }
        public string? RucTramite { get; set; }

        public List<string> CambiosRealizados { get; set; } = new();

    }
}