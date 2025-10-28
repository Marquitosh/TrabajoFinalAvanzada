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

        // GET: api/logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogs()
        {
            try
            {
                var logs = await _context.Logs
                    .OrderByDescending(l => l.Fecha)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        // GET: api/logs/ultimos/10
        [HttpGet("ultimos/{cantidad}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetUltimosLogs(int cantidad = 50)
        {
            try
            {
                var logs = await _context.Logs
                    .OrderByDescending(l => l.Fecha)
                    .Take(cantidad)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetAllLogs()
        {
            try
            {
                var logs = await _context.Logs
                    .OrderByDescending(l => l.Fecha)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
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