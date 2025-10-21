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
    }
}