using MesaPartesDigital.Models;

namespace MesaPartesDigital.Api.Models
{
    public class RegistroDocumentoJuridicoRequest
    {
        // Datos de la Empresa (Jurídica)
        public string VRucEmpresa { get; set; } = string.Empty;
        public string VRazonSocial { get; set; } = string.Empty;

        // Datos del Representante Legal
        public int? ICodPer { get; set; }
        public int ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty;
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty;
        public string VDireccion { get; set; } = string.Empty;
        public string VCodDistrito { get; set; } = string.Empty;

        // Datos del Documento (Comunes)
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;
        public string VNombreAsunto { get; set; } = string.Empty;
        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;
        public string VLink { get; set; } = string.Empty; // El campo que te faltaba

        public List<ArchivoRequest> Archivos { get; set; } = new();
    }

}
