using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using AvanzadaAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelosController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public ModelosController(AvanzadaContext context)
        {
            _context = context;
        }

        // GET: api/modelos (Devuelve TODOS los modelos con nombre de marca)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModeloDto>>> GetModelos()
        {
            var modelos = await _context.Modelos
                .Include(m => m.Marca) // <-- Incluir la marca relacionada
                .OrderBy(m => m.Marca.Nombre).ThenBy(m => m.Nombre) // Ordenar por marca y luego modelo
                .Select(m => new ModeloDto // <-- Mapear al DTO
                {
                    IDModelo = m.IDModelo,
                    Nombre = m.Nombre,
                    IDMarca = m.IDMarca,
                    MarcaNombre = m.Marca.Nombre // <-- Asignar el nombre de la marca
                })
                .ToListAsync();

            return Ok(modelos);
        }

        // GET: api/modelos/marca/5
        [HttpGet("marca/{idMarca}")]
        public async Task<ActionResult<IEnumerable<Modelo>>> GetModelosPorMarca(int idMarca)
        {
            return await _context.Modelos
                                 .Where(m => m.IDMarca == idMarca)
                                 .OrderBy(m => m.Nombre)
                                 .ToListAsync();
        }

        // GET: api/modelos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ModeloDto>> GetModelo(int id)
        {
            // Incluimos la marca para devolverla si es necesario
            var modelo = await _context.Modelos
                .Include(m => m.Marca)
                .Where(m => m.IDModelo == id)
                .Select(m => new ModeloDto
                {
                    IDModelo = m.IDModelo,
                    Nombre = m.Nombre,
                    IDMarca = m.IDMarca,
                    MarcaNombre = m.Marca.Nombre
                })
                .FirstOrDefaultAsync();

            if (modelo == null)
            {
                return NotFound();
            }

            return Ok(modelo);
        }

        // POST: api/modelos
        [HttpPost]
        public async Task<ActionResult<ModeloDto>> PostModelo(ModeloCreateDto modeloDto)
        {
            // Validar DTO explícitamente (aunque [ApiController] ya lo hace)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que la marca exista
            if (!await _context.Marcas.AnyAsync(m => m.IDMarca == modeloDto.IDMarca))
            {
                // Devolver error específico si la marca no existe
                ModelState.AddModelError(nameof(modeloDto.IDMarca), "La marca especificada no existe.");
                return BadRequest(ModelState);
                // O return BadRequest(new { message = "La marca especificada no existe." });
            }

            // Validar si ya existe un modelo con el mismo nombre para esa marca
            if (await _context.Modelos.AnyAsync(m => m.IDMarca == modeloDto.IDMarca && m.Nombre.ToLower() == modeloDto.Nombre.ToLower()))
            {
                // Devolver error específico si el modelo ya existe
                ModelState.AddModelError(nameof(modeloDto.Nombre), $"Ya existe un modelo '{modeloDto.Nombre}' para esta marca.");
                return Conflict(ModelState); // 409 Conflict
                                             // O return Conflict(new { message = $"Ya existe un modelo '{modeloDto.Nombre}' para esta marca." });
            }

            // Mapear DTO al Modelo de base de datos
            var modelo = new Modelo
            {
                Nombre = modeloDto.Nombre,
                IDMarca = modeloDto.IDMarca
            };

            _context.Modelos.Add(modelo);
            await _context.SaveChangesAsync(); // Guardar para obtener el IDModelo generado

            // Crear el DTO de respuesta (ModeloDto)
            var marcaNombre = await _context.Marcas
                                      .Where(m => m.IDMarca == modelo.IDMarca)
                                      .Select(m => m.Nombre)
                                      .FirstOrDefaultAsync() ?? "Desconocida";

            var dtoCreado = new ModeloDto
            {
                IDModelo = modelo.IDModelo, // Usar el ID generado
                Nombre = modelo.Nombre,
                IDMarca = modelo.IDMarca,
                MarcaNombre = marcaNombre
            };

            // Devolver respuesta 201 Created con el DTO
            return CreatedAtAction(nameof(GetModelo), new { id = modelo.IDModelo }, dtoCreado);
        }

        // PUT: api/modelos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutModelo(int id, [FromBody] ModeloCreateDto modeloDto)
        {
            // 1. Validar el DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Buscar la entidad existente en la base de datos
            var modeloEnDb = await _context.Modelos.FindAsync(id);
            if (modeloEnDb == null)
            {
                return NotFound(new { message = "El modelo a actualizar no existe." });
            }

            // 3. Validar lógica de negocio (marca existe, nombre no duplicado)
            if (!await _context.Marcas.AnyAsync(m => m.IDMarca == modeloDto.IDMarca))
            {
                ModelState.AddModelError(nameof(modeloDto.IDMarca), "La marca especificada no existe.");
                return BadRequest(ModelState);
            }

            if (await _context.Modelos.AnyAsync(m => m.IDMarca == modeloDto.IDMarca && m.Nombre.ToLower() == modeloDto.Nombre.ToLower() && m.IDModelo != id))
            {
                ModelState.AddModelError(nameof(modeloDto.Nombre), $"Ya existe otro modelo '{modeloDto.Nombre}' para esta marca.");
                return Conflict(ModelState); // 409 Conflict
            }

            // 4. Mapear DTO a la entidad existente
            modeloEnDb.Nombre = modeloDto.Nombre;
            modeloEnDb.IDMarca = modeloDto.IDMarca;

            // 5. Guardar cambios
            _context.Entry(modeloEnDb).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModeloExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // 6. Devolver 204 No Content (éxito para un PUT)
            return NoContent();
        }

        // DELETE: api/modelos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModelo(int id)
        {
            var modelo = await _context.Modelos.FindAsync(id);
            if (modelo == null)
            {
                return NotFound();
            }

            // Verificar si el modelo está en uso por algún vehículo
            var enUso = await _context.Vehiculos.AnyAsync(v => v.IDModelo == id);
            if (enUso)
            {
                return Conflict(new { message = "No se puede eliminar el modelo porque está asociado a uno o más vehículos." });
            }

            _context.Modelos.Remove(modelo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ModeloExists(int id)
        {
            return _context.Modelos.Any(e => e.IDModelo == id);
        }
    }
}