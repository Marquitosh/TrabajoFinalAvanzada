using AvanzadaAPI.Data;
using AvanzadaAPI.DTOs;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AvanzadaAPI.Controllers
{
    [Route("api/tiposervicios")] // Nueva ruta
    [ApiController]
    public class TipoServiciosController : ControllerBase
    {
        private readonly AvanzadaContext _context;
        private readonly ILogger<TipoServiciosController> _logger;

        public TipoServiciosController(AvanzadaContext context, ILogger<TipoServiciosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/tiposervicios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoServicio>>> GetTipoServicios()
        {
            // Asumimos que creaste el DbSet 'TiposServicio' en tu AvanzadaContext
            return await _context.TiposServicio.ToListAsync();
        }

        // GET: api/tiposervicios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoServicio>> GetTipoServicio(int id)
        {
            var tipoServicio = await _context.TiposServicio.FindAsync(id);

            if (tipoServicio == null)
            {
                return NotFound();
            }

            return tipoServicio;
        }

        // POST: api/tiposervicios
        [HttpPost]
        public async Task<ActionResult<TipoServicio>> PostTipoServicio(TipoServicioDto tipoServicioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe uno con el mismo nombre
            var existe = await _context.TiposServicio
                                      .AnyAsync(ts => ts.Nombre.ToLower() == tipoServicioDto.Nombre.ToLower());
            if (existe)
            {
                return Conflict(new { message = $"Ya existe un tipo de servicio con el nombre '{tipoServicioDto.Nombre}'." });
            }

            var nuevoTipoServicio = new TipoServicio
            {
                Nombre = tipoServicioDto.Nombre,
                Precio = tipoServicioDto.Precio,
                TiempoEstimado = tipoServicioDto.TiempoEstimado,
                Descripcion = tipoServicioDto.Descripcion
            };

            _context.TiposServicio.Add(nuevoTipoServicio);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nuevo TipoServicio creado con ID: {Id}", nuevoTipoServicio.IdTipoServicio);
            return CreatedAtAction(nameof(GetTipoServicio), new { id = nuevoTipoServicio.IdTipoServicio }, nuevoTipoServicio);
        }

        // PUT: api/tiposervicios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoServicio(int id, TipoServicioDto tipoServicioDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipoServicioDb = await _context.TiposServicio.FindAsync(id);
            if (tipoServicioDb == null)
            {
                return NotFound();
            }

            // Verificar si el nuevo nombre ya existe en OTRO registro
            var existeOtro = await _context.TiposServicio
                                       .AnyAsync(ts => ts.Nombre.ToLower() == tipoServicioDto.Nombre.ToLower() && ts.IdTipoServicio != id);
            if (existeOtro)
            {
                return Conflict(new { message = $"Ya existe otro tipo de servicio con el nombre '{tipoServicioDto.Nombre}'." });
            }


            tipoServicioDb.Nombre = tipoServicioDto.Nombre;
            tipoServicioDb.Precio = tipoServicioDto.Precio;
            tipoServicioDb.TiempoEstimado = tipoServicioDto.TiempoEstimado;
            tipoServicioDb.Descripcion = tipoServicioDto.Descripcion;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("TipoServicio ID: {Id} actualizado.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al actualizar TipoServicio ID: {Id}", id);
                throw;
            }

            return NoContent();
        }

        // DELETE: api/tiposervicios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoServicio(int id)
        {
            var tipoServicio = await _context.TiposServicio.FindAsync(id);
            if (tipoServicio == null)
            {
                return NotFound();
            }

            // Verificar si este tipo de servicio está siendo usado en algún Turno (a través de la tabla Servicio)
            var enUso = await _context.Servicios.AnyAsync(s => s.IdTipoServicio == id);
            if (enUso)
            {
                return Conflict(new { message = "No se puede eliminar: Este tipo de servicio está en uso por uno o más turnos." });
            }

            _context.TiposServicio.Remove(tipoServicio);
            await _context.SaveChangesAsync();
            _logger.LogInformation("TipoServicio ID: {Id} eliminado.", id);

            return NoContent();
        }
    }
}