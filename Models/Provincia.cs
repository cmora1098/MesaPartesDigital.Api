namespace MesaPartesDigital.Models
{
    public class Provincia
    {
        public string vCodDepartamento { get; set; } = string.Empty;
        public string vCodProvincia { get; set; } = string.Empty;
        public string vNomProvincia { get; set; } = string.Empty;
        public decimal dcLongitud { get; set; }
        public decimal dcLatitud { get; set; }
    }
}