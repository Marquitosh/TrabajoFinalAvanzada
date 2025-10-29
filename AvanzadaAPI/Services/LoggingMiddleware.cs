// AvanzadaAPI/Services/LoggingMiddleware.cs
using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using System.Diagnostics;

namespace AvanzadaAPI.Services
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AvanzadaContext dbContext)
        {
            var stopwatch = Stopwatch.StartNew();
            var path = context.Request.Path.Value ?? "";

            // Ignorar endpoints que no queremos loggear
            if (path.StartsWith("/swagger") ||
                path.StartsWith("/_framework") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js"))
            {
                await _next(context);
                return;
            }

            try
            {
                // Ejecutar el siguiente middleware
                await _next(context);
                stopwatch.Stop();

                // Registrar la petición exitosa
                await RegistrarLog(context, dbContext, stopwatch.ElapsedMilliseconds, null);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Registrar el error
                await RegistrarLog(context, dbContext, stopwatch.ElapsedMilliseconds, ex);

                // Re-lanzar la excepción para que el error handler global la capture
                throw;
            }
        }

        private async Task RegistrarLog(
            HttpContext context,
            AvanzadaContext dbContext,
            long elapsedMs,
            Exception? ex)
        {
            try
            {
                // Obtener el ID de usuario desde el header personalizado
                string idUsuario = "Anónimo";

                if (context.Request.Headers.TryGetValue("X-Usuario-ID", out var userIdHeader))
                {
                    idUsuario = userIdHeader.ToString();
                }
                // Alternativa: también puedes buscar en claims si usas JWT
                else if (context.User?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId");
                    if (userIdClaim != null)
                    {
                        idUsuario = userIdClaim.Value;
                    }
                }

                // Construir la acción (método + ruta)
                var metodo = context.Request.Method;
                var ruta = context.Request.Path.Value ?? "";
                var accion = $"{metodo} {ruta}";

                // Limitar a 100 caracteres
                if (accion.Length > 100)
                {
                    accion = accion.Substring(0, 97) + "...";
                }

                // Determinar el nivel del log
                string nivel;
                if (ex != null)
                {
                    nivel = "Error";
                }
                else if (context.Response.StatusCode >= 400)
                {
                    nivel = "Warning";
                }
                else
                {
                    nivel = "Info";
                }

                // Construir la descripción
                string descripcion;
                if (ex != null)
                {
                    descripcion = $"Error: {ex.Message}. Tiempo: {elapsedMs}ms";
                }
                else
                {
                    descripcion = $"Status: {context.Response.StatusCode}. Tiempo: {elapsedMs}ms";
                }

                // Limitar a 500 caracteres
                if (descripcion.Length > 500)
                {
                    descripcion = descripcion.Substring(0, 497) + "...";
                }

                // Obtener IP y User Agent
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                // Crear el log
                var log = new Log
                {
                    Fecha = DateTime.UtcNow,
                    Usuario = idUsuario.Length > 100 ? idUsuario.Substring(0, 100) : idUsuario,
                    Accion = accion,
                    Descripcion = descripcion,
                    Nivel = nivel,
                    IPAddress = ipAddress?.Length > 50 ? ipAddress.Substring(0, 50) : ipAddress,
                    UserAgent = userAgent?.Length > 500 ? userAgent.Substring(0, 497) + "..." : userAgent
                };

                // Guardar en la base de datos
                dbContext.Logs.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                // Si falla el logging, no queremos romper la aplicación
                // Solo lo registramos en el logger de consola
                _logger.LogError(logEx, "Error al guardar log en la base de datos");
            }
        }
    }

    // Extension method para facilitar el registro del middleware
    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}