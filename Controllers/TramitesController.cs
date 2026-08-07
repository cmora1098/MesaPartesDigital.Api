using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MesaPartesDigital.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TramitesController : ControllerBase
    {
        private readonly TramiteService _tramiteService;

        public TramitesController(TramiteService tramiteService)
        {
            _tramiteService = tramiteService;
        }

        /// <summary>
        /// Consulta el detalle de un trámite mediante su número y periodo.
        /// </summary>
        /// <param name="nroTramite">Número de trámite (ej: 2049)</param>
        /// <param name="periodo">Periodo o año (ej: 2021)</param>
        /// <returns>Lista de documentos asociados al trámite</returns>
        [HttpGet("consultar-cut")]
        [ProducesResponseType(typeof(IEnumerable<TramiteConsultaDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConsultarCut([FromQuery] int nroTramite, [FromQuery] int periodo)
        {
            if (nroTramite <= 0 || periodo <= 0)
            {
                return BadRequest("El número de trámite y el periodo deben ser válidos.");
            }

            try
            {
                var resultado = await _tramiteService.ConsultarCutAsync(nroTramite, periodo);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al procesar la consulta: {ex.Message}");
            }
        }
    }
}
