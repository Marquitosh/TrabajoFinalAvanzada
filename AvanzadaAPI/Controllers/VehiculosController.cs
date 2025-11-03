using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculosController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public VehiculosController(AvanzadaContext context)
        {
            _context = context;
        }

        // GET: api/Vehiculos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehiculo>>> GetVehiculos()
        {
            return await _context.Vehiculos
                .Include(v => v.TipoCombustible)
                .Include(v => v.Usuario)
                .ToListAsync();
        }

        // GET: api/Vehiculos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehiculo>> GetVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos
                .Include(v => v.TipoCombustible)
                .Include(v => v.Usuario)
                .FirstOrDefaultAsync(v => v.IDVehiculo == id);

            if (vehiculo == null)
            {
                return NotFound();
            }

            return vehiculo;
        }

        // GET: api/Vehiculos/Usuario/5
        [HttpGet("Usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<Vehiculo>>> GetVehiculosByUsuario(int userId)
        {
            return await _context.Vehiculos
                .Include(v => v.TipoCombustible)
                .Where(v => v.IDUsuario == userId)
                .ToListAsync();
        }

        // PUT: api/Vehiculos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehiculo(int id, Vehiculo vehiculo)
        {
            if (id != vehiculo.IDVehiculo)
            {
                return BadRequest();
            }

            _context.Entry(vehiculo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehiculoExists(id))
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

        // POST: api/Vehiculos
        [HttpPost]
        public async Task<ActionResult<Vehiculo>> PostVehiculo(Vehiculo vehiculo)
        {
            _context.Vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehiculo", new { id = vehiculo.IDVehiculo }, vehiculo);
        }

        // DELETE: api/Vehiculos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            var enUso = await _context.Turnos.AnyAsync(t => t.IDVehiculo == id);
            if (enUso)
            {
                // Devolver un error 409 Conflict (conflicto) con un mensaje claro
                return Conflict(new { message = "No se puede eliminar el vehículo porque tiene turnos asociados." });
            }

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("TiposCombustible")]
        public async Task<ActionResult<IEnumerable<TipoCombustible>>> GetTiposCombustible()
        {
            return await _context.TiposCombustible.ToListAsync();
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.IDVehiculo == id);
        }
    }
}