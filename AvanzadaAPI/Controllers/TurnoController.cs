using AvanzadaAPI.Data;
using AvanzadaAPI.DTOs;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

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
        public async Task<ActionResult<IEnumerable<TurnoAdminDto>>> GetTurnos()
        {
            try
            {
                var turnosDto = await _context.Turnos
                    .Include(t => t.Usuario) // Incluir Usuario para NombreCompleto
                    .Include(t => t.Vehiculo)
                    .Include(t => t.EstadoTurno)
                    .Include(t => t.Servicios)
                        .ThenInclude(s => s.TipoServicio)
                    .OrderByDescending(t => t.FechaHora)
                    .Select(t => new TurnoAdminDto
                    {
                        IdTurno = t.IDTurno,
                        Usuario = t.Usuario != null ? t.Usuario.NombreCompleto : $"Usuario ID: {t.IDUsuario}", // Obtener nombre
                        Vehiculo = (t.Vehiculo != null
                            ? $"{t.Vehiculo.Marca} {t.Vehiculo.Modelo} ({t.Vehiculo.Patente})"
                            : "Vehículo no especificado"),
                        FechaHora = t.FechaHora,
                        Estado = t.EstadoTurno != null ? t.EstadoTurno.Descripcion : "Desconocido",
                        Observaciones = t.Observaciones,
                        DuracionTotal = t.Servicios.Sum(s => s.TipoServicio != null ? s.TipoServicio.TiempoEstimado : 0),
                        ServiciosNombres = t.Servicios
                                            .Where(s => s.TipoServicio != null)
                                            .Select(s => s.TipoServicio!.Nombre)
                                            .ToList() ?? new List<string>()
                    })
                    .ToListAsync();

                return Ok(turnosDto); // Devuelve la lista del nuevo DTO
            }
            catch (Exception ex)
            {
                // Considera loggear el error (ex)
                return StatusCode(500, $"Error interno al obtener los turnos: {ex.Message}");
            }
        }

        // GET: api/Turnos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Turno>> GetTurno(int id)
        {
            var turno = await _context.Turnos
                .Include(t => t.Usuario)
                .Include(t => t.Vehiculo)
                .Include(t => t.EstadoTurno)
                .Include(t => t.Servicios)
                    .ThenInclude(s => s.TipoServicio)
                .FirstOrDefaultAsync(t => t.IDTurno == id);

            if (turno == null)
            {
                return NotFound();
            }

            return turno;
        }

        // GET: api/Turnos/Usuario/5
        [HttpGet("Usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<TurnoUsuarioDto>>> GetTurnosByUsuario(int userId)
        {
            var turnosDto = await _context.Turnos
        .Include(t => t.Vehiculo)
        .Include(t => t.EstadoTurno)
        .Include(t => t.Servicios)
            .ThenInclude(s => s.TipoServicio)
        .Where(t => t.IDUsuario == userId)
        .OrderByDescending(t => t.FechaHora)
        .Select(t => new TurnoUsuarioDto
        {
            IdTurno = t.IDTurno,
            Vehiculo = (t.Vehiculo != null
                ? $"{t.Vehiculo.Marca} {t.Vehiculo.Modelo} ({t.Vehiculo.Patente})"
                : "Vehículo no especificado"),
            FechaHora = t.FechaHora,
            Estado = t.EstadoTurno != null ? t.EstadoTurno.Descripcion : "Desconocido",
            Observaciones = t.Observaciones,

            // Calcula la duración y extrae los nombres aquí mismo
            DuracionTotal = t.Servicios.Sum(s => s.TipoServicio != null ? s.TipoServicio.TiempoEstimado : 0),
            ServiciosNombres = t.Servicios
                                .Where(s => s.TipoServicio != null) // Evitar nulls
                                .Select(s => s.TipoServicio!.Nombre) // Obtener solo el nombre
                                .ToList() ?? new List<string>()
        })
        .ToListAsync();

            return Ok(turnosDto);
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
        public async Task<ActionResult<Turno>> PostTurno(CreateTurnoDto createTurnoDto)
        {
            // Validar que los TipoServicio existan
            var tiposServicioCount = await _context.TiposServicio
                                    .CountAsync(ts => createTurnoDto.IdTipoServicios.Contains(ts.IdTipoServicio));

            if (tiposServicioCount != createTurnoDto.IdTipoServicios.Count)
            {
                return BadRequest("Uno o más IDs de servicio no son válidos.");
            }

            // Iniciar una transacción de base de datos
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Crear el objeto Turno principal
                var newTurno = new Turno
                {
                    IDUsuario = createTurnoDto.IDUsuario,
                    IDVehiculo = createTurnoDto.IDVehiculo,
                    FechaHora = createTurnoDto.FechaHora,
                    IDEstadoTurno = createTurnoDto.IDEstadoTurno, // Ej: 1 (Pendiente)
                    Observaciones = "" // Se puede añadir después
                };

                _context.Turnos.Add(newTurno);
                await _context.SaveChangesAsync(); // Guardar para obtener el IDTurno

                // 2. Crear los Servicios asociados
                // Asumimos 1 = "Pendiente" según tu seed.sql de EstadoServicio
                var defaultEstadoServicioId = 1;

                foreach (var idTipoServicio in createTurnoDto.IdTipoServicios)
                {
                    var newServicio = new Servicio
                    {
                        IdTurno = newTurno.IDTurno, // ID del turno recién creado
                        IdTipoServicio = idTipoServicio,
                        IdEstadoServicio = defaultEstadoServicioId
                    };
                    _context.Servicios.Add(newServicio);
                }

                // 3. Guardar los servicios
                await _context.SaveChangesAsync();

                // 4. Confirmar la transacción
                await transaction.CommitAsync();

                // Devolver el turno creado (con su nuevo ID)
                return CreatedAtAction(nameof(GetTurno), new { id = newTurno.IDTurno }, newTurno);
            }
            catch (Exception ex)
            {
                // Si algo falla (en la creación del Turno o de los Servicios), revertir todo
                await transaction.RollbackAsync();

                // Idealmente loggear el error (ex)

                return StatusCode(500, "Error interno al crear el turno y sus servicios.");
            }
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