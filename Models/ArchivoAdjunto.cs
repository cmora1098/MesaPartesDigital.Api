using Microsoft.AspNetCore.Components.Forms;

namespace MesaPartesDigital.Api.Models
{
    public class ArchivoAdjunto
    {
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";
        public string Tamano { get; set; } = "";
        public byte[] Contenido { get; set; } = Array.Empty<byte>(); // O usa string Base64 si lo prefieres
        public IBrowserFile? FileBrowser { get; set; }
    }
}
