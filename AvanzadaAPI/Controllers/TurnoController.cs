using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnosController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public TurnosController(AvanzadaContext context)
        {
            _context = context;
        }

        // GET: api/Turnos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Turno>>> GetTurnos()
        {
            try
            {
                var turnos = await _context.Turnos
                    .Include(t => t.Usuario)
                    .Include(t => t.Vehiculo)
                    .Include(t => t.Servicio)
                    .Include(t => t.EstadoTurno)
                    .OrderByDescending(t => t.Fecha)
                    .ThenBy(t => t.Hora)
                    .ToListAsync();

                return Ok(turnos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/Turnos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Turno>> GetTurno(int id)
        {
            var turno = await _context.Turnos
                .Include(t => t.Usuario)
                .Include(t => t.Vehiculo)
                .Include(t => t.Servicio)
                .Include(t => t.EstadoTurno)
                .FirstOrDefaultAsync(t => t.IDTurno == id);

            if (turno == null)
            {
                return NotFound();
            }

            return turno;
        }

        // GET: api/Turnos/Usuario/5
        [HttpGet("Usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<Turno>>> GetTurnosByUsuario(int userId)
        {
            return await _context.Turnos
                .Include(t => t.Vehiculo)
                .Include(t => t.Servicio)
                .Include(t => t.EstadoTurno)
                .Where(t => t.IDUsuario == userId)
                .ToListAsync();
        }

        // PUT: api/Turnos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTurno(int id, Turno turno)
        {
            if (id != turno.IDTurno)
            {
                return BadRequest();
            }

            _context.Entry(turno).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TurnoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Turnos
        [HttpPost]
        public async Task<ActionResult<Turno>> PostTurno(Turno turno)
        {
            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTurno", new { id = turno.IDTurno }, turno);
        }

        // DELETE: api/Turnos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTurno(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
            {
                return NotFound();
            }

            _context.Turnos.Remove(turno);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/turnos/5/confirmar
        [HttpPut("{id}/confirmar")]
        public async Task<IActionResult> ConfirmarTurno(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
            {
                return NotFound();
            }

            // Buscar el ID del estado "Confirmado"
            var estadoConfirmado = await _context.EstadosTurno
                .FirstOrDefaultAsync(e => e.Descripcion == "Confirmado");

            if (estadoConfirmado != null)
            {
                turno.IDEstadoTurno = estadoConfirmado.IDEstadoTurno;
                await _context.SaveChangesAsync();
                return NoContent();
            }

            return BadRequest("No se encontró el estado 'Confirmado'");
        }

        // PUT: api/turnos/5/cancelar
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarTurno(int id)
        {
            var turno = await _context.Turnos.FindAsync(id);
            if (turno == null)
            {
                return NotFound();
            }

            var estadoCancelado = await _context.EstadosTurno
                .FirstOrDefaultAsync(e => e.Descripcion == "Cancelado");

            if (estadoCancelado != null)
            {
                turno.IDEstadoTurno = estadoCancelado.IDEstadoTurno;
                await _context.SaveChangesAsync();
                return NoContent();
            }

            return BadRequest("No se encontró el estado 'Cancelado'");
        }

        private bool TurnoExists(int id)
        {
            return _context.Turnos.Any(e => e.IDTurno == id);
        }
    }
}