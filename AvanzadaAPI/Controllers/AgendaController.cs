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
    public class AgendaController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public AgendaController(AvanzadaContext context)
        {
            _context = context;
        }

        // ENDPOINT 1: Obtener el horario de atención
        // GET: api/agenda/horarios
        [HttpGet("horarios")]
        public async Task<ActionResult<HorariosDisponibles>> GetHorariosDisponibles()
        {
            // Asumimos que solo hay UN registro de horarios.
            var horarios = await _context.HorariosDisponibles.FirstOrDefaultAsync();
            if (horarios == null)
            {
                return NotFound("No se ha configurado un horario de atención.");
            }
            return Ok(horarios);
        }

        // ENDPOINT 2: Obtener los turnos ocupados (calculando su duración)
        // GET: api/agenda/turnosocupados
        [HttpGet("turnosocupados")]
        public async Task<ActionResult<IEnumerable<TurnoOcupadoDto>>> GetTurnosOcupados()
        {
            var turnos = await _context.Turnos
                // Incluimos solo los turnos que están "activos" (ej. no cancelados)
                // Asumamos ID 1 = Pendiente, ID (a definir) = Confirmado
                .Where(t => t.IDEstadoTurno == 1) // Ajusta esto a tus IDs de estado
                .Include(t => t.Servicios)
                    .ThenInclude(s => s.TipoServicio)
                .Select(t => new TurnoOcupadoDto
                {
                    FechaHoraInicio = t.FechaHora,
                    // Calculamos la duración total SUMANDO el TiempoEstimado
                    // de cada TipoServicio asociado a los Servicios del Turno
                    DuracionTotalMinutos = t.Servicios.Sum(s => s.TipoServicio.TiempoEstimado)
                })
                .ToListAsync();

            return Ok(turnos);
        }

        // ENDPOINT 3: Obtener solo los TipoServicio seleccionados
        // GET: api/agenda/tiposervicios?ids=1&ids=3
        [HttpGet("tiposervicios")]
        public async Task<ActionResult<IEnumerable<TipoServicio>>> GetTipoServiciosByIds([FromQuery] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return BadRequest("No se proporcionaron IDs de servicios.");
            }

            var servicios = await _context.TiposServicio
                                          .Where(ts => ids.Contains(ts.IdTipoServicio))
                                          .ToListAsync();

            return Ok(servicios);
        }

        // GET: api/agenda/diasdisponibles?duracionTurno=90
        [HttpGet("diasdisponibles")]
        public async Task<ActionResult<IEnumerable<DiaDisponibleDto>>> GetDiasDisponibles([FromQuery] int duracionTurno)
        {
            if (duracionTurno <= 0)
            {
                return BadRequest("La duración del turno debe ser mayor a 0.");
            }

            // 1. Obtener la configuración de horarios (asumimos 1 solo registro)
            var horarioAtencion = await _context.HorariosDisponibles.FirstOrDefaultAsync();
            if (horarioAtencion == null)
            {
                return StatusCode(500, "El horario de atención no está configurado.");
            }

            // 2. Obtener TODOS los turnos ocupados (Pendientes o Confirmados)
            var turnosOcupados = await _context.Turnos
                .Where(t => (t.IDEstadoTurno == 1 || t.IDEstadoTurno == 2) && t.FechaHora > DateTime.Now)
                .Include(t => t.Servicios)
                    .ThenInclude(s => s.TipoServicio)
                .Select(t => new
                {
                    Inicio = t.FechaHora,
                    Fin = t.FechaHora.AddMinutes(t.Servicios.Sum(s => s.TipoServicio.TiempoEstimado))
                })
                .ToListAsync();

            // 3. Generar la lista de 30 días hábiles
            var diasDisponibles = new List<DiaDisponibleDto>();
            var fechaActual = DateTime.Today.AddDays(1); // Empezar desde mañana
            var diasEncontrados = 0;
            var diasMaxBusqueda = 180; // Límite de seguridad (6 meses)
            var culturaES = new CultureInfo("es-ES");
            const int slotIntervaloMinutos = 30; // Intervalo de los slots (09:00, 09:30, ...)

            while (diasEncontrados < 30 && diasMaxBusqueda > 0)
            {
                var (inicio, fin) = GetHorariosParaDia(fechaActual.DayOfWeek, horarioAtencion);

                // Si el negocio está abierto ese día
                if (inicio.HasValue && fin.HasValue)
                {
                    var slotsDelDia = new List<HorarioDisponibleDto>();
                    var inicioMinutos = (int)inicio.Value.TotalMinutes;
                    var finMinutos = (int)fin.Value.TotalMinutes;

                    // Iterar por cada slot del día (09:00, 09:30, 10:00...)
                    for (int slotInicioMin = inicioMinutos; slotInicioMin < finMinutos; slotInicioMin += slotIntervaloMinutos)
                    {
                        var slotFinMin = slotInicioMin + duracionTurno;

                        // Check 1: El turno cabe dentro del horario de atención?
                        if (slotFinMin > finMinutos)
                        {
                            break; // Este slot (y los siguientes) no caben
                        }

                        // Convertir minutos a DateTime
                        var nuevoSlotInicio = fechaActual.AddMinutes(slotInicioMin);
                        var nuevoSlotFin = fechaActual.AddMinutes(slotFinMin);

                        // Check 2: El turno se superpone con un turno existente?
                        bool estaOcupado = turnosOcupados.Any(turno =>
                            nuevoSlotInicio < turno.Fin && nuevoSlotFin > turno.Inicio
                        );

                        if (!estaOcupado)
                        {
                            // Si el slot está libre, añadirlo
                            slotsDelDia.Add(new HorarioDisponibleDto
                            {
                                Hora = nuevoSlotInicio.ToString("HH:mm"),
                                FechaHoraISO = nuevoSlotInicio
                            });
                        }
                    }

                    // Si se encontró al menos 1 slot, el día está disponible
                    if (slotsDelDia.Count > 0)
                    {
                        diasEncontrados++;
                        diasDisponibles.Add(new DiaDisponibleDto
                        {
                            // Formato: "lunes, 3 de noviembre" (lo capitalizamos en JS)
                            Texto = culturaES.DateTimeFormat
                                .GetDayName(fechaActual.DayOfWeek) + ", " +
                                fechaActual.ToString("d 'de' MMMM", culturaES),
                            Slots = slotsDelDia
                        });
                    }
                }

                fechaActual = fechaActual.AddDays(1);
                diasMaxBusqueda--;
            }

            return Ok(diasDisponibles);
        }

        // Función Helper para obtener horarios
        private (TimeSpan? inicio, TimeSpan? fin) GetHorariosParaDia(DayOfWeek dia, HorariosDisponibles h)
        {
            switch (dia)
            {
                case DayOfWeek.Monday: return (h.LunesInicio, h.LunesFin);
                case DayOfWeek.Tuesday: return (h.MartesInicio, h.MartesFin);
                case DayOfWeek.Wednesday: return (h.MiercolesInicio, h.MiercolesFin);
                case DayOfWeek.Thursday: return (h.JuevesInicio, h.JuevesFin);
                case DayOfWeek.Friday: return (h.ViernesInicio, h.ViernesFin);
                case DayOfWeek.Saturday: return (h.SabadoInicio, h.SabadoFin);
                case DayOfWeek.Sunday: return (h.DomingoInicio, h.DomingoFin);
                default: return (null, null);
            }
        }
    }
}