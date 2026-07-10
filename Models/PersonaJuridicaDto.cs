namespace MesaPartesDigital.Models
{
    public class PersonaJuridicaDto
    {
        public int iCodPer { get; set; }
        public int iCodTipoPer { get; set; }
        public string vDocPer { get; set; } = null!;
        public string? vEmpresa { get; set; }
        public string? vEmail { get; set; }
        public string? vTelefono { get; set; }
        public string? vCodDistrito { get; set; }
        public string? vDireccion { get; set; }
        public bool bCorreoVerificado { get; set; }
        public int? iCodPerRepresentante { get; set; }
        public bool bActivo { get; set; }
    }
}