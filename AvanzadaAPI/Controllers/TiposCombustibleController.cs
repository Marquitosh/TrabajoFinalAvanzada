using AvanzadaAPI.Data;
using AvanzadaAPI.DTOs;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Considera añadir [Authorize(Roles = "Admin")]
    public class TiposCombustibleController : ControllerBase
    {
        private readonly AvanzadaContext _context;
        private readonly ILogger<TiposCombustibleController> _logger;

        public TiposCombustibleController(AvanzadaContext context, ILogger<TiposCombustibleController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/TiposCombustible
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoCombustible>>> GetTiposCombustible()
        {
            return await _context.TiposCombustible.OrderBy(tc => tc.Descripcion).ToListAsync();
        }

        // GET: api/TiposCombustible/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoCombustible>> GetTipoCombustible(int id)
        {
            var tipoCombustible = await _context.TiposCombustible.FindAsync(id);

            if (tipoCombustible == null)
            {
                return NotFound();
            }

            return tipoCombustible;
        }

        // POST: api/TiposCombustible
        [HttpPost]
        public async Task<ActionResult<TipoCombustible>> PostTipoCombustible(TipoCombustibleDto tipoCombustibleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar si ya existe uno con la misma descripción (ignorando mayúsculas/minúsculas)
            var existe = await _context.TiposCombustible
                                       .AnyAsync(tc => tc.Descripcion.ToLower() == tipoCombustibleDto.Descripcion.ToLower());
            if (existe)
            {
                return Conflict(new { message = $"Ya existe un tipo de combustible con la descripción '{tipoCombustibleDto.Descripcion}'." });
            }


            var nuevoTipoCombustible = new TipoCombustible
            {
                Descripcion = tipoCombustibleDto.Descripcion
            };

            _context.TiposCombustible.Add(nuevoTipoCombustible);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Nuevo TipoCombustible creado con ID: {Id}", nuevoTipoCombustible.IDCombustible);
            return CreatedAtAction(nameof(GetTipoCombustible), new { id = nuevoTipoCombustible.IDCombustible }, nuevoTipoCombustible);
        }

        // PUT: api/TiposCombustible/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoCombustible(int id, TipoCombustibleDto tipoCombustibleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tipoCombustibleDb = await _context.TiposCombustible.FindAsync(id);
            if (tipoCombustibleDb == null)
            {
                return NotFound();
            }

            // Verificar si la nueva descripción ya existe en OTRO registro
            var existeOtro = await _context.TiposCombustible
                                       .AnyAsync(tc => tc.Descripcion.ToLower() == tipoCombustibleDto.Descripcion.ToLower() && tc.IDCombustible != id);
            if (existeOtro)
            {
                return Conflict(new { message = $"Ya existe otro tipo de combustible con la descripción '{tipoCombustibleDto.Descripcion}'." });
            }

            tipoCombustibleDb.Descripcion = tipoCombustibleDto.Descripcion;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("TipoCombustible ID: {Id} actualizado.", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al actualizar TipoCombustible ID: {Id}", id);
                // Podríamos verificar si aún existe, pero FindAsync ya lo hizo.
                throw; // Re-lanzar para que el middleware global maneje el 500
            }

            return NoContent(); // Estándar para PUT exitoso
        }

        // DELETE: api/TiposCombustible/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoCombustible(int id)
        {
            var tipoCombustible = await _context.TiposCombustible.FindAsync(id);
            if (tipoCombustible == null)
            {
                return NotFound();
            }

            // Verificar si este tipo de combustible está siendo usado por algún vehículo
            var enUso = await _context.Vehiculos.AnyAsync(v => v.IDCombustible == id);
            if (enUso)
            {
                return Conflict(new { message = "No se puede eliminar: Este tipo de combustible está en uso por uno o más vehículos." });
            }

            _context.TiposCombustible.Remove(tipoCombustible);
            await _context.SaveChangesAsync();
            _logger.LogInformation("TipoCombustible ID: {Id} eliminado.", id);

            return NoContent();
        }
    }
}