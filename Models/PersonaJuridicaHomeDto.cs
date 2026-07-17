using MesaPartesDigital.Models;

namespace MesaPartesDigital.Api.Models
{
    // Registro desde la pantalla Home
    public class PersonaJuridicaHomeDto
    {
        // DATOS DE LA EMPRESA
        public string vRucEmpresa { get; set; } = string.Empty;
        public string VRazonSocial { get; set; } = string.Empty;

        // DATOS DEL REPRESENTANTE LEGAL
        public int ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty;
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty;
        public string VDireccion { get; set; } = string.Empty;
        public string VCodDistrito { get; set; } = string.Empty;

        // DATOS DEL DOCUMENTO
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;
        public string VNombreAsunto { get; set; } = string.Empty;
        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;

        // --- 4. LISTA DE ARCHIVOS ---
        public int? ICodPer { get; set; }
        public List<ArchivoRequestPJH> Archivos { get; set; } = new();
    }

    public class ArchivoRequestPJH
    {
        public string VRutaDoc { get; set; } = string.Empty;
        public bool BTipo { get; set; } // true (1) = Principal, false (0) = Anexo
    }

    public class RegistroDocumentoResponsePJH
    {
        public int ICodDoc { get; set; }
        public int ICodAsunto { get; set; }
        public int ICodPer { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MailSeguimiento { get; set; } = string.Empty;
        public string? VAutoGenerado { get; set; } = string.Empty; // Puede ser null en caso de Anexos (Hijos)
    }

}
