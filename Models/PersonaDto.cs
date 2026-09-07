namespace MesaPartesDigital.Api.Models
{
    public class PersonaDto
    {
        public int ICodPer { get; set; }
        public int? ICodTipoPer { get; set; }
        public int? ICodTipoDocPer { get; set; }
        public string VDocPer { get; set; } = string.Empty;
        public string VNombres { get; set; } = string.Empty;
        public string VApellidoPaterno { get; set; } = string.Empty;
        public string VApellidoMaterno { get; set; } = string.Empty;
        public string VNombreCompleto { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string VTelefono { get; set; } = string.Empty;
        public string VEmpresa { get; set; } = string.Empty;
        public bool BActivo { get; set; }
        public string VCodDistrito { get; set; } = string.Empty;
        public string VDireccion { get; set; } = string.Empty;
        public bool BCorreoVerificado { get; set; }
        public int? ICodPerRepresentante { get; set; }
        public string? VCodDepartamento { get; set; } // Cambiado a Mayúscula
        public string? VCodProvincia { get; set; }    // Cambiado a Mayúscula
    }
}
