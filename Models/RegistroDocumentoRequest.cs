using System;
using System.Collections.Generic;

namespace MesaPartesDigital.Models
{
    public class RegistroDocumentoRequest
    {
        // --- 1. DATOS DEL REMITENTE (Para T_Persona) ---
        public int ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty;
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty; 
        public string VDireccion { get; set; } = string.Empty;
        public string VCodDistrito { get; set; } = string.Empty;

        // --- 2. DATOS DEL ASUNTO Y DOCUMENTO (Para T_Asunto y T_Documento) ---
        public int ICodTipoDoc { get; set; }
        public string VNroDoc { get; set; } = string.Empty;
        public DateTime DFecDoc { get; set; } = DateTime.Today;
        public string VNombreAsunto { get; set; } = string.Empty; // La Sumilla
        public string VReferencia { get; set; } = string.Empty;
        public string VNroPagFolios { get; set; } = string.Empty;

        // --- 3. LISTA DE ARCHIVOS (Para T_Documento / Anexos) ---
        // Aquí centralizamos la subida de archivos, ya no como variables sueltas
        public List<ArchivoRequest> Archivos { get; set; } = new();
    }

    public class ArchivoRequest
    {
        public string VRutaDoc { get; set; } = string.Empty;
        public bool BTipo { get; set; } // true (1) = Principal, false (0) = Anexo
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