using AvanzadaAPI.Data;
using AvanzadaAPI.DTOs;
using AvanzadaAPI.Models;
using AvanzadaAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AvanzadaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AvanzadaContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UsuariosController> _logger;
        private readonly IConfiguration _configuration;

        public UsuariosController(AvanzadaContext context,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<UsuariosController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
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

                // Guardar en una transacción separada para evitar conflictos
                var affectedRows = await _context.SaveChangesAsync();

                if (affectedRows > 0)
                {
                    _logger.LogInformation("Log guardado exitosamente: {Accion}", accion);
                }
                else
                {
                    _logger.LogWarning("SaveChanges no afectó ninguna fila al guardar log: {Accion}", accion);
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos al registrar log. Accion: {Accion}, InnerException: {Inner}",
                    accion, dbEx.InnerException?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general al registrar log en base de datos. Accion: {Accion}", accion);
            }
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios.Include(u => u.NivelUsuario).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.NivelUsuario)
                    .FirstOrDefaultAsync(u => u.IDUsuario == id);

                if (usuario == null)
                {
                    return NotFound();
                }

                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioLoginDto>> Login(LoginRequest loginRequest)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.NivelUsuario)
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

                if (usuario == null || !VerifyPassword(loginRequest.Password, usuario.Contraseña))
                {
                    // LOG: Intento de login fallido
                    await RegistrarLog(
                        "Warning",
                        "Login fallido",
                        $"Email: {loginRequest.Email}"
                    );
                    return Unauthorized("Credenciales inválidas");
                }

                // LOG: Login exitoso
                await RegistrarLog(
                    "Info",
                    "Login exitoso",
                    $"Usuario: {usuario.Nombre} {usuario.Apellido} (ID: {usuario.IDUsuario})"
                );

                var response = new UsuarioLoginDto
                {
                    IDUsuario = usuario.IDUsuario,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    IDNivel = usuario.IDNivel,
                    Foto = usuario.Foto,
                    RolNombre = usuario.NivelUsuario?.RolNombre
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error en login", ex.Message);
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut("updateprofile/{id}")]
        public async Task<ActionResult<Usuario>> UpdateProfile(int id, [FromBody] UpdateProfileDto usuarioActualizado)
        {
            try
            {
                if (usuarioActualizado == null)
                {
                    return BadRequest("Datos de usuario no pueden ser nulos");
                }

                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                if (!string.IsNullOrEmpty(usuarioActualizado.Email) && usuarioActualizado.Email != usuario.Email)
                {
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioActualizado.Email && u.IDUsuario != id))
                    {
                        return BadRequest("El email ya está en uso por otro usuario");
                    }
                }

                // Construir descripción de cambios
                var cambios = new List<string>();
                if (!string.IsNullOrEmpty(usuarioActualizado.Nombre) && usuarioActualizado.Nombre != usuario.Nombre)
                    cambios.Add($"Nombre: {usuario.Nombre} → {usuarioActualizado.Nombre}");

                if (!string.IsNullOrEmpty(usuarioActualizado.Apellido) && usuarioActualizado.Apellido != usuario.Apellido)
                    cambios.Add($"Apellido: {usuario.Apellido} → {usuarioActualizado.Apellido}");

                if (!string.IsNullOrEmpty(usuarioActualizado.Email) && usuarioActualizado.Email != usuario.Email)
                    cambios.Add($"Email: {usuario.Email} → {usuarioActualizado.Email}");

                if (usuarioActualizado.Telefono != null && usuarioActualizado.Telefono != usuario.Telefono)
                    cambios.Add($"Teléfono: {usuario.Telefono} → {usuarioActualizado.Telefono}");

                if (!string.IsNullOrEmpty(usuarioActualizado.ContraseñaString))
                    cambios.Add("Contraseña actualizada");

                if (usuarioActualizado.Foto != null && usuarioActualizado.Foto.Length > 0)
                    cambios.Add("Foto actualizada");

                // Aplicar cambios
                if (!string.IsNullOrEmpty(usuarioActualizado.Nombre))
                    usuario.Nombre = usuarioActualizado.Nombre;

                if (!string.IsNullOrEmpty(usuarioActualizado.Apellido))
                    usuario.Apellido = usuarioActualizado.Apellido;

                if (!string.IsNullOrEmpty(usuarioActualizado.Email))
                    usuario.Email = usuarioActualizado.Email;

                if (usuarioActualizado.Telefono != null)
                    usuario.Telefono = usuarioActualizado.Telefono;

                if (!string.IsNullOrEmpty(usuarioActualizado.ContraseñaString))
                    usuario.Contraseña = HashPassword(usuarioActualizado.ContraseñaString);

                if (usuarioActualizado.Foto != null && usuarioActualizado.Foto.Length > 0)
                    usuario.Foto = usuarioActualizado.Foto;

                await _context.SaveChangesAsync();

                // LOG: Perfil actualizado
                if (cambios.Any())
                {
                    await RegistrarLog(
                        "Info",
                        "Perfil actualizado",
                        $"Usuario ID: {id} - {string.Join(", ", cambios)}"
                    );
                }

                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                if (usuario.NivelUsuario == null)
                {
                    usuario.NivelUsuario = await _context.NivelesUsuario.FindAsync(usuario.IDNivel);
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error actualizar perfil", $"ID: {id} - {ex.Message}");
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            try
            {
                if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                {
                    // LOG: Intento de registro con email duplicado
                    await RegistrarLog(
                        "Warning",
                        "Intento registro email duplicado",
                        $"Email: {usuario.Email}"
                    );
                    return BadRequest("El email ya está registrado");
                }

                if (usuario.IDNivel == 0)
                {
                    usuario.IDNivel = 1;
                }

                var nivelExiste = await _context.NivelesUsuario.AnyAsync(n => n.IDNivel == usuario.IDNivel);
                if (!nivelExiste)
                {
                    return BadRequest($"El nivel de usuario con ID {usuario.IDNivel} no existe");
                }

                if (!string.IsNullOrEmpty(usuario.ContraseñaString))
                {
                    usuario.Contraseña = HashPassword(usuario.ContraseñaString);
                }
                else
                {
                    return BadRequest("La contraseña es requerida");
                }

                usuario.FechaRegistro = DateTime.Now;

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // LOG: Usuario creado exitosamente
                await RegistrarLog(
                    "Info",
                    "Usuario creado",
                    $"ID: {usuario.IDUsuario}, Nombre: {usuario.Nombre} {usuario.Apellido}, Email: {usuario.Email}"
                );

                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                return CreatedAtAction("GetUsuario", new { id = usuario.IDUsuario }, usuario);
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error crear usuario", ex.Message);
#if DEBUG
                return StatusCode(500, new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
#else
                return StatusCode(500, "Error interno del servidor al registrar el usuario");
#endif
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound();
                }

                // Guardar datos antes de eliminar
                var datosUsuario = $"ID: {usuario.IDUsuario}, Nombre: {usuario.Nombre} {usuario.Apellido}, Email: {usuario.Email}";

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                // LOG: Usuario eliminado
                await RegistrarLog(
                    "Critical",
                    "Usuario eliminado",
                    datosUsuario
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error eliminar usuario", $"ID: {id} - {ex.Message}");
                return StatusCode(500, "Error al eliminar usuario");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Usuario>> UpdateUsuario(int id, [FromBody] UpdateUsuarioDto usuarioActualizado)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                if (usuarioActualizado.IDNivel.HasValue)
                {
                    var nivelExiste = await _context.NivelesUsuario.AnyAsync(n => n.IDNivel == usuarioActualizado.IDNivel.Value);
                    if (!nivelExiste)
                    {
                        return BadRequest("El nivel de usuario especificado no existe");
                    }

                    var nivelAnterior = usuario.IDNivel;
                    usuario.IDNivel = usuarioActualizado.IDNivel.Value;

                    // LOG: Nivel actualizado
                    await RegistrarLog(
                        "Info",
                        "Nivel usuario actualizado",
                        $"Usuario ID: {id} - Nivel: {nivelAnterior} → {usuario.IDNivel}"
                    );
                }

                await _context.SaveChangesAsync();

                usuario.Contraseña = Array.Empty<byte>();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error actualizar usuario", $"ID: {id} - {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut("{id}/rol")]
        public async Task<ActionResult> ActualizarRolUsuario(int id, [FromBody] ActualizarRolDto actualizarRolDto)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                var rolExiste = await _context.NivelesUsuario.AnyAsync(n => n.IDNivel == actualizarRolDto.IDNivel);
                if (!rolExiste)
                {
                    return BadRequest("El rol especificado no existe");
                }

                var rolAnterior = usuario.IDNivel;
                usuario.IDNivel = actualizarRolDto.IDNivel;
                await _context.SaveChangesAsync();

                // LOG: Rol actualizado
                await RegistrarLog(
                    "Warning",
                    "Rol actualizado",
                    $"Usuario ID: {id} - Rol: {rolAnterior} → {actualizarRolDto.IDNivel}"
                );

                return Ok(new { mensaje = "Rol actualizado correctamente" });
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error actualizar rol", $"ID: {id} - {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    // LOG: Intento recuperación email no existente
                    await RegistrarLog(
                        "Warning",
                        "Recuperación email inexistente",
                        $"Email: {request.Email}"
                    );
                    return Ok(new { message = "Si el email existe, se enviarán instrucciones de recuperación." });
                }

                var token = await _tokenService.GeneratePasswordResetTokenAsync(usuario.IDUsuario);

                var emailSent = await _emailService.SendPasswordResetEmailAsync(
                    usuario.Email,
                    usuario.NombreCompleto,
                    token
                );

                if (!emailSent)
                {
                    _logger.LogError("Falló el envío de email de recuperación a {Email}", usuario.Email);
                    return StatusCode(500, new { message = "Error al enviar el email de recuperación." });
                }

                // LOG: Email recuperación enviado
                await RegistrarLog(
                    "Info",
                    "Email recuperación enviado",
                    $"Usuario: {usuario.Nombre} {usuario.Apellido} (ID: {usuario.IDUsuario})"
                );

                return Ok(new { message = "Se han enviado instrucciones de recuperación a tu email." });
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error recuperar contraseña", $"Email: {request.Email} - {ex.Message}");
                return StatusCode(500, new { message = "Error interno del servidor." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordRequest request)
        {
            try
            {
                var resetToken = await _tokenService.ValidateTokenAsync(request.Token);
                if (resetToken == null)
                {
                    // LOG: Token inválido o expirado
                    await RegistrarLog(
                        "Warning",
                        "Token reset inválido",
                        $"Token utilizado: {request.Token.Substring(0, Math.Min(10, request.Token.Length))}..."
                    );
                    return BadRequest(new { message = "Token inválido o expirado." });
                }

                resetToken.Usuario.Contraseña = HashPassword(request.NewPassword);
                await _tokenService.MarkTokenAsUsedAsync(request.Token);
                await _context.SaveChangesAsync();

                // LOG: Contraseña reseteada exitosamente
                await RegistrarLog(
                    "Warning",
                    "Contraseña reseteada",
                    $"Usuario ID: {resetToken.Usuario.IDUsuario}"
                );

                return Ok(new { message = "Contraseña actualizada correctamente." });
            }
            catch (Exception ex)
            {
                await RegistrarLog("Error", "Error reset contraseña", ex.Message);
                return StatusCode(500, new { message = "Error interno del servidor." });
            }
        }

        // Métodos sin logs (consultas de solo lectura)
        [HttpGet("{id}/vehiculos/count")]
        public async Task<ActionResult<int>> GetVehiculosCount(int id)
        {
            try
            {
                var count = await _context.Vehiculos
                    .Where(v => v.IDUsuario == id)
                    .CountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando vehículos para usuario {ID}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/turnos/proximo")]
        public async Task<ActionResult<ProximoTurnoDto>> GetProximoTurno(int id)
        {
            try
            {
                var proximoTurno = await _context.Turnos
                    .Where(t => t.IDUsuario == id &&
                               (t.Fecha > DateTime.Today ||
                               (t.Fecha == DateTime.Today && t.Hora >= DateTime.Now.TimeOfDay)))
                    .OrderBy(t => t.Fecha)
                    .ThenBy(t => t.Hora)
                    .Select(t => new ProximoTurnoDto
                    {
                        Fecha = t.Fecha,
                        Hora = t.Hora,
                    })
                    .FirstOrDefaultAsync();

                if (proximoTurno == null)
                {
                    return Ok(new ProximoTurnoDto());
                }

                return Ok(proximoTurno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo próximo turno para usuario {ID}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/turnos/count")]
        public async Task<ActionResult<int>> GetTurnosCount(int id)
        {
            try
            {
                var count = await _context.Turnos
                    .Where(t => t.IDUsuario == id &&
                               (t.Fecha > DateTime.Today ||
                               (t.Fecha == DateTime.Today && t.Hora >= DateTime.Now.TimeOfDay)))
                    .CountAsync();
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error contando turnos para usuario {ID}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}/vehiculos")]
        public async Task<ActionResult<IEnumerable<VehiculoViewModel>>> GetVehiculosByUsuario(int id)
        {
            try
            {
                var vehiculos = await _context.Vehiculos
                    .Where(v => v.IDUsuario == id)
                    .Include(v => v.TipoCombustible)
                    .Select(v => new VehiculoViewModel
                    {
                        IDVehiculo = v.IDVehiculo,
                        Marca = v.Marca,
                        Modelo = v.Modelo,
                        Patente = v.Patente,
                        Year = v.Year
                    })
                    .ToListAsync();
                return Ok(vehiculos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo vehículos para usuario {ID}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("validate-reset-token")]
        public async Task<ActionResult> ValidateResetToken(ValidateTokenRequest request)
        {
            try
            {
                var resetToken = await _tokenService.ValidateTokenAsync(request.Token);
                if (resetToken == null)
                {
                    return BadRequest(new { message = "Token inválido o expirado." });
                }

                return Ok(new
                {
                    valid = true,
                    email = resetToken.Usuario.Email,
                    message = "Token válido."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token: {Token}", request.Token);
                return StatusCode(500, new { message = "Error validando token." });
            }
        }

        private bool VerifyPassword(string password, byte[] storedHash)
        {
            using var sha256 = SHA256.Create();
            var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return passwordHash.SequenceEqual(storedHash);
        }

        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        // DTOs
        public class ActualizarRolDto
        {
            public int IDNivel { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class UpdateUsuarioDto
        {
            public int? IDNivel { get; set; }
        }

        public class ForgotPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class ResetPasswordRequest
        {
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ValidateTokenRequest
        {
            public string Token { get; set; } = string.Empty;
        }

        public class ProximoTurnoDto
        {
            public DateTime? Fecha { get; set; }
            public TimeSpan? Hora { get; set; }
            public string? Descripcion { get; set; }
            public DateTime? FechaHora => Fecha?.Add(Hora ?? TimeSpan.Zero);
        }

        public class VehiculoViewModel
        {
            public int IDVehiculo { get; set; }
            public string Marca { get; set; } = string.Empty;
            public string Modelo { get; set; } = string.Empty;
            public string Patente { get; set; } = string.Empty;
            public int Year { get; set; }
        }
    }
}