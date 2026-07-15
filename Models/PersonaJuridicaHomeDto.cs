using MesaPartesDigital.Models;

namespace MesaPartesDigital.Api.Models
{
    // Registro desde la pantalla Home
    public class PersonaJuridicaHomeDto
    {
        // --- 1. DATOS DEL REPRESENTANTE LEGAL (Para T_Persona) ---
        public int ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty; // DNI del Representante
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty;
        public string VDireccion { get; set; } = string.Empty;
        public string VCodDistrito { get; set; } = string.Empty;

        // --- 2. DATOS DE LA EMPRESA (Para T_Tramite) ---
        public string VRUC { get; set; } = string.Empty;
        public string VRazonSocial { get; set; } = string.Empty;

        // --- 3. DATOS DEL ASUNTO Y DOCUMENTO (Para T_Asunto y T_Documento) ---
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;
        public string VNombreAsunto { get; set; } = string.Empty;
        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;

        // --- 4. LISTA DE ARCHIVOS ---
        public int? ICodPer { get; set; }
        public List<ArchivoRequest> Archivos { get; set; } = new();
    }

}
