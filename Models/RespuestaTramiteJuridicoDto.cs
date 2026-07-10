namespace MesaPartesDigital.Models
{
    public class RespuestaTramiteJuridicoDto
    {
        public int iCodDoc { get; set; }
        public int iCodAsunto { get; set; }
        public string Status { get; set; } = null!;
        public string MailSeguimiento { get; set; } = null!;
        public string? vAutoGenerado { get; set; }
    }
}