using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NivelesUsuarioController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public NivelesUsuarioController(AvanzadaContext context)
        {
            _context = context;
        }

        // GET: api/nivelesusuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NivelUsuario>>> GetNivelesUsuario()
        {
            try
            {
                var niveles = await _context.NivelesUsuario.ToListAsync();
                return Ok(niveles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/nivelesusuario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NivelUsuario>> GetNivelUsuario(int id)
        {
            try
            {
                var nivelUsuario = await _context.NivelesUsuario.FindAsync(id);

                if (nivelUsuario == null)
                {
                    return NotFound();
                }

                return Ok(nivelUsuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}