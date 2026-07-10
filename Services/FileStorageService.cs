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
}
