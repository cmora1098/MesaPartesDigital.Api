namespace MesaPartesDigital.Api.Models
{
    public class RegistroPersonaJuridicaDto
    {
        // I. Datos de Empresa
        public string RucEmpresa { get; set; }
        public string RazonSocial { get; set; }

        // II. Datos de Representante
        public int CodTipoDocRep { get; set; }
        public string DocRep { get; set; }
        public string NombresRep { get; set; }
        public string ApellidoPaternoRep { get; set; }
        public string ApellidoMaternoRep { get; set; }
        public string EmailRep { get; set; }
        public string TelefonoRep { get; set; }
        public string DireccionRep { get; set; }
        public string CodDistritoRep { get; set; }

        // III. Datos de Documento
        public int? CodAsunto { get; set; }
        public string RutaDoc { get; set; }
        public int CodTipoDoc { get; set; }
        public string NroDoc { get; set; }
        public DateTime FecDoc { get; set; }
        public string Referencia { get; set; }
        public string NroPagFolios { get; set; }
        public bool btipo { get; set; }
        public string Link { get; set; }
    }
}
