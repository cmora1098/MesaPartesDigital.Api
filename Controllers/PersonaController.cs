
using Microsoft.AspNetCore.Mvc;
using MesaPartesDigital.Api.Models;
using MesaPartesDigital.Api.Services;

namespace MesaPartesDigital.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : ControllerBase
    {
        private readonly IPersonaService _personaService;

        public PersonaController(IPersonaService personaService)
        {
            _personaService = personaService;
        }

        // GET: api/Persona/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPersonaPorId(int id)
        {
            var persona = await _personaService.ObtenerPorIdAsync(id);

            if (persona == null)
            {
                return NotFound(new
                {
                    mensaje = "Persona no encontrada."
                });
            }

            return Ok(persona);
        }

        // PUT: api/Persona/5
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPersona(
            int id,
            [FromBody] PersonaDto persona)
        {
            if (id != persona.ICodPer)
            {
                return BadRequest(new
                {
                    mensaje = "El ID de la persona no coincide."
                });
            }

            var existe = await _personaService.ObtenerPorIdAsync(id);

            if (existe == null)
            {
                return NotFound(new
                {
                    mensaje = "Persona no encontrada."
                });
            }

            var actualizado = await _personaService.ActualizarAsync(persona);

            if (!actualizado)
            {
                return BadRequest(new
                {
                    mensaje = "No se pudo actualizar la información."
                });
            }

            return Ok(new
            {
                mensaje = "Los datos personales fueron actualizados correctamente."
            });
        }
    }
}