namespace MesaPartesDigital.Api.Models
{
    public class TramiteConsultaDto
    {
        public int Ide_Documento { get; set; } // Coincide con IDE_DOCUMENTO
        public string? Cut { get; set; }
        public string? Estado { get; set; }
        public string? Sede { get; set; }
        public string? Ventanilla { get; set; }
        public string? Fec_Ingreso { get; set; } // Coincide con FEC_INGRESO
        public string? Nro_Doc { get; set; }     // Coincide con NRO_DOC
        public string? Rem_Emp { get; set; }     // Coincide con REM_EMP
        public string? Rem_Per { get; set; }     // Coincide con REM_PER
        public string? Asunto { get; set; }
        public string? Txt_Tiene { get; set; }   // Coincide con TXT_TIENE
        public string? Ruta_Atencion { get; set; } // Coincide con RUTA_ATENCION
    }
}
