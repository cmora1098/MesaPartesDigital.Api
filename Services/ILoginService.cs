using System.Threading.Tasks;

namespace MesaPartesDigital.Services
{
    public interface ILoginService
    {
         Task<LoginResultDto> ValidarCredencialesAsync(string email, string password); 
         Task<GestionCredencialesResultDto> GestionarCredencialesAsync(string email, string? codigoOtp, int tipoAccion);
    }

    public class LoginResultDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = "Error no especificado"; // Valor por defecto útil
        public int ICodPer { get; set; }
        public string VNombreCompleto { get; set; } = string.Empty;
        public string VEmail { get; set; } = string.Empty;
        public string Token { get; set; }
    }

    public class GestionCredencialesResultDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string? PasswordGenerado { get; set; }
        public string? EmailDestino { get; set; }
    }
}