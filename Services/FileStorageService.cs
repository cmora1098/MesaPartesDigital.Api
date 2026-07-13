namespace MesaPartesDigital.Services;

public sealed class FileStorageService
{
    private readonly string _rootPath;
    private readonly long _maxBytes;

    public FileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["Storage:RootPath"] ?? throw new InvalidOperationException("Storage:RootPath no configurado.");
        _maxBytes = (configuration.GetValue<long?>("Storage:MaxFileSizeMb") ?? 50) * 1024 * 1024;
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file.Length == 0) throw new InvalidOperationException("El archivo está vacío.");
        if (file.Length > _maxBytes) throw new InvalidOperationException("El archivo supera el tamaño permitido.");

        var folder = Path.Combine(_rootPath, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
        Directory.CreateDirectory(folder);
        var safeName = Path.GetFileNameWithoutExtension(file.FileName).Replace(' ', '_');
        var finalName = $"{Guid.NewGuid():N}__{safeName}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(folder, finalName);
        await using var output = File.Create(fullPath);
        await file.CopyToAsync(output, ct);
        return fullPath;
    }

    public async Task<string> SaveBase64Async(string nombreOriginal, string base64Data, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(base64Data)) throw new ArgumentException("El contenido base64 está vacío.");

        // Limpieza del string base64 si incluye encabezado de tipo de dato
        if (base64Data.Contains(","))
        {
            base64Data = base64Data.Split(',')[1];
        }

        byte[] fileBytes = Convert.FromBase64String(base64Data);
        if (fileBytes.Length > _maxBytes) throw new InvalidOperationException("El archivo supera el tamaño permitido.");

        var folder = Path.Combine(_rootPath, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MM"));
        Directory.CreateDirectory(folder);

        var safeName = Path.GetFileNameWithoutExtension(nombreOriginal).Replace(' ', '_');
        var extension = Path.GetExtension(nombreOriginal);
        var finalName = $"{Guid.NewGuid():N}__{safeName}{extension}";
        var fullPath = Path.Combine(folder, finalName);

        await File.WriteAllBytesAsync(fullPath, fileBytes, ct);

        return fullPath;
    }
}
