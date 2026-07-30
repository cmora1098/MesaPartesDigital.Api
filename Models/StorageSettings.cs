namespace MesaPartesDigital.Api.Models
{
    public class StorageSettings
    {
        public string RootPath { get; set; } = string.Empty;
        public string PathView { get; set; } = string.Empty;
        public string RelativePath { get; set; }
        public int MaxFileSizeMb { get; set; }
    }
}
