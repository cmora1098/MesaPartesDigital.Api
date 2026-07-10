namespace MesaPartesDigital.Models
{
    public class RegistroDocumentoRequest
    {
        // Campos que ya usábamos para el Trámite Interno Dashboard:
        public int ICodPer { get; set; }
        public string VEmail { get; set; } = string.Empty;
        public int ICodAsunto { get; set; }
        public string VRutaDoc { get; set; } = string.Empty;
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;
        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;
        public bool BTipo { get; set; }
        public string? VLink { get; set; }

        // 🟢 AGREGA ESTOS CAMPOS NUEVAMENTE PARA SOLUCIONAR LOS ERRORES EN OTRAS VISTAS:
        public int ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty;
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty;
        public string VDireccion { get; set; } = string.Empty;
        public string VCodDistrito { get; set; } = string.Empty;
    }

    public class RegistroDocumentoResponse
    {
        public int ICodDoc { get; set; }
        public int ICodAsunto { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MailSeguimiento { get; set; } = string.Empty;
        public string? VAutoGenerado { get; set; } = string.Empty; // Puede ser null en caso de Anexos (Hijos)
    }
}