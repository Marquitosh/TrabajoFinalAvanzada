using AvanzadaAPI.Data;
using AvanzadaAPI.DTOs;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Considera añadir autorización [Authorize(Roles = "Admin")] si implementas autenticación/autorización en la API
    public class ConfiguracionController : ControllerBase
    {
        private readonly AvanzadaContext _context;
        private readonly ILogger<ConfiguracionController> _logger;

        public ConfiguracionController(AvanzadaContext context, ILogger<ConfiguracionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/configuracion/horarios
        [HttpGet("horarios")]
        public async Task<ActionResult<HorariosDisponiblesDto>> GetHorarios()
        {
            // Asumimos que solo existe un registro de horarios (ID=1 basado en el seed)
            var horarios = await _context.HorariosDisponibles.FirstOrDefaultAsync(h => h.IdHorario == 1);

            if (horarios == null)
            {
                _logger.LogWarning("No se encontró el registro de HorariosDisponibles con ID=1.");
                // Podrías crear uno por defecto aquí si lo deseas, o devolver NotFound
                return NotFound("No se encontró la configuración de horarios.");
            }

            // --- LOG ANTES DE MAPEAR ---
            _logger.LogWarning($"[API GET] Leyendo de DB -> LunesInicio: {horarios.LunesInicio?.ToString("c") ?? "null"}"); // "c" formato constante

            var horariosDto = MapToDto(horarios);

            // --- LOG DESPUÉS DE MAPEAR ---
            _logger.LogWarning($"[API GET] Mapeado a DTO -> LunesInicio: {horariosDto.LunesInicio ?? "null"}");

            return Ok(horariosDto);
        }

        // PUT: api/configuracion/horarios
        [HttpPut("horarios")]
        public async Task<IActionResult> UpdateHorarios(HorariosDisponiblesDto horariosDto)
        {
            // Validar modelo (incluyendo las expresiones regulares para HH:mm)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Buscar el registro existente (asumimos ID=1)
            var horariosDb = await _context.HorariosDisponibles.FirstOrDefaultAsync(h => h.IdHorario == 1);

            if (horariosDb == null)
            {
                _logger.LogError("Intento de actualizar HorariosDisponibles (ID=1) que no existe.");
                return NotFound("No se encontró la configuración de horarios para actualizar.");
            }

            try
            {
                MapToModel(horariosDto, horariosDb); // Convierte string a TimeSpan?

                // --- LOG ANTES DE GUARDAR ---
                _logger.LogWarning($"[API PUT] Guardando en DB -> LunesInicio: {horariosDb.LunesInicio?.ToString("c") ?? "null"}");

                _context.Entry(horariosDb).State = EntityState.Modified;
                await _context.SaveChangesAsync(); // <-- GUARDADO

                _logger.LogInformation("[API PUT] Horarios de atención actualizados correctamente.");
                return NoContent();
            }
            catch (FormatException ex)
            {
                // Error al parsear alguna de las horas
                _logger.LogWarning(ex, "Error de formato al actualizar horarios.");
                return BadRequest(new { message = $"Error en el formato de hora: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar horarios.");
                return StatusCode(500, "Error interno al guardar la configuración de horarios.");
            }
        }

        // --- Métodos Helper de Mapeo ---

        private HorariosDisponiblesDto MapToDto(HorariosDisponibles model)
        {
            // Formato "HH:mm" asegura dos dígitos para hora y minuto
            return new HorariosDisponiblesDto
            {
                IdHorario = model.IdHorario,
                LunesInicio = model.LunesInicio?.ToString(@"hh\:mm"),
                LunesFin = model.LunesFin?.ToString(@"hh\:mm"),
                MartesInicio = model.MartesInicio?.ToString(@"hh\:mm"),
                MartesFin = model.MartesFin?.ToString(@"hh\:mm"),
                MiercolesInicio = model.MiercolesInicio?.ToString(@"hh\:mm"),
                MiercolesFin = model.MiercolesFin?.ToString(@"hh\:mm"),
                JuevesInicio = model.JuevesInicio?.ToString(@"hh\:mm"),
                JuevesFin = model.JuevesFin?.ToString(@"hh\:mm"),
                ViernesInicio = model.ViernesInicio?.ToString(@"hh\:mm"),
                ViernesFin = model.ViernesFin?.ToString(@"hh\:mm"),
                SabadoInicio = model.SabadoInicio?.ToString(@"hh\:mm"),
                SabadoFin = model.SabadoFin?.ToString(@"hh\:mm"),
                DomingoInicio = model.DomingoInicio?.ToString(@"hh\:mm"),
                DomingoFin = model.DomingoFin?.ToString(@"hh\:mm")
            };
        }

        private void MapToModel(HorariosDisponiblesDto dto, HorariosDisponibles model)
        {
            // Parsea HH:mm a TimeSpan?, lanza FormatException si falla
            model.LunesInicio = ParseTimeSpan(dto.LunesInicio);
            model.LunesFin = ParseTimeSpan(dto.LunesFin);
            model.MartesInicio = ParseTimeSpan(dto.MartesInicio);
            model.MartesFin = ParseTimeSpan(dto.MartesFin);
            model.MiercolesInicio = ParseTimeSpan(dto.MiercolesInicio);
            model.MiercolesFin = ParseTimeSpan(dto.MiercolesFin);
            model.JuevesInicio = ParseTimeSpan(dto.JuevesInicio);
            model.JuevesFin = ParseTimeSpan(dto.JuevesFin);
            model.ViernesInicio = ParseTimeSpan(dto.ViernesInicio);
            model.ViernesFin = ParseTimeSpan(dto.ViernesFin);
            model.SabadoInicio = ParseTimeSpan(dto.SabadoInicio);
            model.SabadoFin = ParseTimeSpan(dto.SabadoFin);
            model.DomingoInicio = ParseTimeSpan(dto.DomingoInicio);
            model.DomingoFin = ParseTimeSpan(dto.DomingoFin);
        }

        private TimeSpan? ParseTimeSpan(string? timeString)
        {
            // Si el string es nulo o vacío, devuelve null
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return null;
            }
            // Intenta parsear exactamente en formato HH:mm
            return TimeSpan.ParseExact(timeString, @"hh\:mm", CultureInfo.InvariantCulture);
        }

        // Necesitas añadir este using al principio del archivo:
        // using System.Globalization;
    }
}