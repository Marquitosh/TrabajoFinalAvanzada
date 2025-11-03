using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AvanzadaAPI.DTOs;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiculosController : ControllerBase
    {
        private readonly AvanzadaContext _context;
        private readonly ILogger<VehiculosController> _logger;

        public VehiculosController(AvanzadaContext context, ILogger<VehiculosController> logger)
        {
            _context = context;
            _logger = logger;
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
        public async Task<IActionResult> PutVehiculo(int id, [FromBody] VehiculoCreateDto vehiculoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vehiculoEnDb = await _context.Vehiculos.FindAsync(id);

            if (vehiculoEnDb == null)
            {
                return NotFound();
            }

            // Mapear desde el DTO (vehiculoDto) al objeto de la BD (vehiculoEnDb)
            vehiculoEnDb.Year = vehiculoDto.Year;
            vehiculoEnDb.Patente = vehiculoDto.Patente;
            vehiculoEnDb.IDCombustible = vehiculoDto.IDCombustible;
            vehiculoEnDb.Observaciones = vehiculoDto.Observaciones;
            vehiculoEnDb.IDMarca = vehiculoDto.IDMarca;
            vehiculoEnDb.IDModelo = vehiculoDto.IDModelo;
            // IMPORTANTE: No mapeamos vehiculoEnDb.IDUsuario. Permanece el original.

            _context.Entry(vehiculoEnDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) // El catch genérico que ya tenías
            {
                _logger.LogError(ex, "Error en SaveChanges en PUT Vehiculo {Id}", id);
                return StatusCode(500, new { message = "Error interno al guardar." });
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