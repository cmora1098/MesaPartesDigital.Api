using MesaPartesDigital.Models;

namespace MesaPartesDigital.Api.Models
{
    public class RegTramitePersNaturalDto
    {
        public int ICodPer { get; set; }
        public string VEmail { get; set; } = string.Empty;

        // Datos del formulario de documento (Cuerpo del SP)
        public int ICodAsunto { get; set; } // El SP lo recibe como IN/OUT
        public string VRutaDoc { get; set; } = string.Empty;
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;
        public string VNombreAsunto { get; set; } = string.Empty; // Usado para T_Asunto
        public string VReferencia { get; set; } = string.Empty;   // Usado para T_Documento
        public string VNroPagFolios { get; set; } = string.Empty;
        public bool BTipo { get; set; }                          // 1=Principal, 0=Anexo
        public string? VLink { get; set; }                       // Campo opcional incluido en el SP
          
        public List<ArchivoRequestTPN> Archivos { get; set; } = new(); 
    }

    public class ArchivoRequestTPN
    {
        public string VRutaDoc { get; set; } = string.Empty;
        public bool BTipo { get; set; } // true (1) = Principal, false (0) = Anexo
    }

    public class RegistroDocumentoResponseTPN
    {
        public int ICodDoc { get; set; }
        public int ICodAsunto { get; set; }
        public int ICodPer { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MailSeguimiento { get; set; } = string.Empty;
        public string? VAutoGenerado { get; set; } = string.Empty; // Puede ser null en caso de Anexos (Hijos)
    }
}
