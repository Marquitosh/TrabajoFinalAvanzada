using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AvanzadaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly AvanzadaContext _context;

        public LogsController(AvanzadaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] string? nivel,
            [FromQuery] string? usuario)
        {
            try
            {
                var query = _context.Logs.AsQueryable();

                // Aplicar filtros
                if (fechaDesde.HasValue)
                    query = query.Where(l => l.Fecha >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                {
                    // Incluir todo el día hasta las 23:59:59
                    var fechaHastaFin = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(l => l.Fecha <= fechaHastaFin);
                }

                if (!string.IsNullOrEmpty(nivel))
                    query = query.Where(l => l.Nivel == nivel);

                if (!string.IsNullOrEmpty(usuario))
                    query = query.Where(l => l.Usuario.Contains(usuario));

                // Ordenar por fecha descendente (más reciente primero)
                var logs = await query
                    .OrderByDescending(l => l.Fecha)
                    .Select(l => new
                    {
                        l.IDLog,
                        l.Fecha,
                        l.Usuario,
                        l.Accion,
                        l.Descripcion,
                        l.Nivel,
                        l.IPAddress
                    })
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los logs", error = ex.Message });
            }
        }

        // Método auxiliar para registrar logs
        private async Task RegistrarLog(string nivel, string accion, string descripcion)
        {
            try
            {
                var usuario = User?.Identity?.Name ?? "Anónimo";

                // Obtener IP Address de forma segura
                string? ipAddress = null;
                if (HttpContext?.Connection?.RemoteIpAddress != null)
                {
                    var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                    ipAddress = ip.Length > 50 ? ip.Substring(0, 50) : ip;
                }

                // Obtener User Agent de forma segura
                string? userAgent = null;
                if (HttpContext?.Request?.Headers != null &&
                    HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgentValue))
                {
                    var ua = userAgentValue.ToString();
                    userAgent = ua.Length > 500 ? ua.Substring(0, 500) : ua;
                }

                var log = new Log
                {
                    Fecha = DateTime.Now,
                    Nivel = nivel.Length > 20 ? nivel.Substring(0, 20) : nivel,
                    Accion = accion.Length > 100 ? accion.Substring(0, 100) : accion,
                    Descripcion = descripcion.Length > 500 ? descripcion.Substring(0, 497) + "..." : descripcion,
                    Usuario = usuario.Length > 100 ? usuario.Substring(0, 100) : usuario,
                    IPAddress = ipAddress,
                    UserAgent = userAgent
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();

                // Debug: puedes ver esto en la consola
                Console.WriteLine($"✅ Log guardado - Accion: {accion}");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"❌ Error DB al guardar log - Accion: {accion}, Error: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al guardar log - Accion: {accion}, Error: {ex.Message}");
            }
        }
    }
}