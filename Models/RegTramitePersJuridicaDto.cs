namespace MesaPartesDigital.Api.Models
{
    public class RegTramitePersJuridicaDto
    {
        public int ICodPer { get; set; }
        public string VEmail { get; set; } = string.Empty;

        // I. DATOS DE LA EMPRESA (Necesarios para sincronizar T_Contribuyente y T_Persona)
        public string VRucEmpresa { get; set; } = string.Empty;
 
        // II. DATOS DEL DOCUMENTO (Necesarios para el INSERT en T_Documento)
        public int ICodAsunto { get; set; } // Usado para el ID del trámite en el SP (IN/OUT)
        public string VRutaDoc { get; set; } = string.Empty;
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;

        // III. DATOS DEL ASUNTO (Necesarios para el INSERT en T_Asunto en la primera iteración)
        public string VNombreAsunto { get; set; } = string.Empty;
        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;

        // IV. CONTROL Y METADATA
        public bool BTipo { get; set; } // 1=Principal, 0=Anexo
        public string? VLink { get; set; }

        // V. COLECCIÓN DE ARCHIVOS
        // Esta lista se usa en el Controlador para iterar y registrar cada documento
        public List<ArchivoRequestTPJ> Archivos { get; set; } = new();
    }

    public class ArchivoRequestTPJ
    {
        public string VRutaDoc { get; set; } = string.Empty;
        public bool BTipo { get; set; } // true (1) = Principal, false (0) = Anexo
    }

    public class RegistroDocumentoResponseTPJ
    {
        public int ICodDoc { get; set; }
        public int ICodAsunto { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MailSeguimiento { get; set; } = string.Empty;
        public string? VAutoGenerado { get; set; }
    }
}
