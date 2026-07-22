using Microsoft.AspNetCore.Components.Forms;

namespace MesaPartesDigital.Api.Models
{
    public class FormFileBrowserAdapter : IBrowserFile
    {
        private readonly IFormFile _formFile;

        public FormFileBrowserAdapter(IFormFile formFile)
        {
            _formFile = formFile;
        }

        public string Name => _formFile.FileName;
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public long Size => _formFile.Length;
        public string ContentType => _formFile.ContentType;

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return _formFile.OpenReadStream();
        }
    }
}
