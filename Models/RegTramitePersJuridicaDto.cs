namespace MesaPartesDigital.Api.Models
{
    public class RegTramitePersJuridicaDto
    {
        // I. DATOS DE LA EMPRESA
        public string VRucEmpresa { get; set; } = string.Empty;
        public string VRazonSocial { get; set; } = string.Empty;

        // II. DATOS DEL DOCUMENTO
        public int ICodAsunto { get; set; }
        public string VRutaDoc { get; set; } = string.Empty;
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;

        // Propiedad agregada para coincidir con el parámetro @vNombreAsunto del SP
        public string VNombreAsunto { get; set; } = string.Empty;

        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;
        public bool BTipo { get; set; } // 1=Principal, 0=Anexo
        public string? VLink { get; set; }

        // Archivos (Se procesan en el componente y se asignan aquí)
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
