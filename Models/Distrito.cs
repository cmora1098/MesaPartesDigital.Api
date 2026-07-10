namespace MesaPartesDigital.Models
{
    public class Distrito
    {
        public string vCodDepartamento { get; set; } = string.Empty;
        public string vCodProvincia { get; set; } = string.Empty;
        public string vCodDistrito { get; set; } = string.Empty;
        public string vNomDistrito { get; set; } = string.Empty;
        public decimal dcLongitud { get; set; }
        public decimal dcLatitud { get; set; }
    }
}