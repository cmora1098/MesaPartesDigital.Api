using MesaPartesDigital.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers;

[ApiController]
[Route("api/archivos")]
public sealed class ArchivosController(FileStorageService storage) : ControllerBase
{
    [HttpPost("subir")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Subir(IFormFile archivo, CancellationToken ct)
        => Ok(new { ruta = await storage.SaveAsync(archivo, ct) });
}
