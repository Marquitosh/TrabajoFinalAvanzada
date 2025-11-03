using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public MarcasController(AvanzadaContext context)
        {
            _context = context;
        }

        // GET: api/marcas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Marca>>> GetMarcas()
        {
            return await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();
        }

        // GET: api/marcas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Marca>> GetMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);

            if (marca == null)
            {
                return NotFound();
            }

            return marca;
        }

        // POST: api/marcas
        [HttpPost]
        public async Task<ActionResult<Marca>> PostMarca(Marca marca)
        {
            // Validar si ya existe una marca con el mismo nombre (ignorando mayúsculas/minúsculas)
            if (await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == marca.Nombre.ToLower()))
            {
                return Conflict(new { message = $"Ya existe una marca con el nombre '{marca.Nombre}'." });
            }

            _context.Marcas.Add(marca);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMarca), new { id = marca.IDMarca }, marca);
        }

        // PUT: api/marcas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMarca(int id, Marca marca)
        {
            if (id != marca.IDMarca)
            {
                return BadRequest();
            }

            // Validar si el nuevo nombre ya existe en otra marca
            if (await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == marca.Nombre.ToLower() && m.IDMarca != id))
            {
                return Conflict(new { message = $"Ya existe otra marca con el nombre '{marca.Nombre}'." });
            }

            _context.Entry(marca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MarcaExists(id))
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

        // DELETE: api/marcas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarca(int id)
        {
            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null)
            {
                return NotFound();
            }

            // Verificar si la marca está en uso por algún modelo
            var enUso = await _context.Modelos.AnyAsync(m => m.IDMarca == id);
            if (enUso)
            {
                return Conflict(new { message = "No se puede eliminar la marca, está en uso por uno o más modelos." });
            }

            _context.Marcas.Remove(marca);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MarcaExists(int id)
        {
            return _context.Marcas.Any(e => e.IDMarca == id);
        }
    }
}