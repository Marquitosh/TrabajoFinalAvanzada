using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        public UsuariosController(AvanzadaContext context)
        {
            _context = context;
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
                Console.WriteLine($"=== INICIO GetUsuario API - ID: {id} ===");

                var usuario = await _context.Usuarios
                    .Include(u => u.NivelUsuario)
                    .FirstOrDefaultAsync(u => u.IDUsuario == id);

                if (usuario == null)
                {
                    Console.WriteLine("Usuario no encontrado");
                    return NotFound();
                }

                Console.WriteLine($"Usuario encontrado: {usuario.Nombre} {usuario.Apellido}");
                Console.WriteLine($"Foto en BD: {(usuario.Foto != null ? $"{usuario.Foto.Length} bytes" : "null")}");

                // No devolver la contraseña
                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                Console.WriteLine("=== FIN GetUsuario API (ÉXITO) ===");
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetUsuario API: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UsuarioViewModel>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Buscar usuario por email
                var usuario = await _context.Usuarios
                    .Include(u => u.NivelUsuario)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    return Unauthorized(new { message = "Usuario no encontrado" });
                }

                // Verificar contraseña
                using var sha256 = SHA256.Create();
                var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
                var passwordHash = sha256.ComputeHash(passwordBytes);

                if (!passwordHash.SequenceEqual(usuario.Contraseña))
                {
                    return Unauthorized(new { message = "Contraseña incorrecta" });
                }

                // Devolver usuario sin información sensible
                return Ok(new UsuarioViewModel
                {
                    IDUsuario = usuario.IDUsuario,
                    Email = usuario.Email,
                    Telefono = usuario.Telefono,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    NivelDescripcion = usuario.NivelUsuario?.Descripcion,
                    Foto = usuario.Foto != null && usuario.Foto.Length > 0
                   ? Convert.ToBase64String(usuario.Foto)
                   : null
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en login: {ex}");
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("updateprofile/{id}")]
        public async Task<ActionResult<Usuario>> UpdateProfile(int id, [FromBody] UpdateProfileDto usuarioActualizado)
        {
            try
            {
                Console.WriteLine($"=== INICIO UpdateProfile API ===");
                Console.WriteLine($"ID recibido: {id}");
                Console.WriteLine($"Datos recibidos en DTO:");
                Console.WriteLine($"- Nombre: {usuarioActualizado?.Nombre ?? "null"}");
                Console.WriteLine($"- Apellido: {usuarioActualizado?.Apellido ?? "null"}");
                Console.WriteLine($"- Email: {usuarioActualizado?.Email ?? "null"}");
                Console.WriteLine($"- Telefono: {usuarioActualizado?.Telefono ?? "null"}");
                Console.WriteLine($"- ContraseñaString está vacía: {string.IsNullOrEmpty(usuarioActualizado?.ContraseñaString)}");
                Console.WriteLine($"- Foto es null: {usuarioActualizado?.Foto == null}");

                // Validar que el objeto no sea nulo
                if (usuarioActualizado == null)
                {
                    Console.WriteLine("ERROR: usuarioActualizado es null");
                    return BadRequest("Datos de usuario no pueden ser nulos");
                }

                // Buscar el usuario existente
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario == null)
                {
                    Console.WriteLine("ERROR: Usuario no encontrado");
                    return NotFound("Usuario no encontrado");
                }
                Console.WriteLine($"Usuario encontrado: {usuario.Email}");

                // Verificar email único solo si se está actualizando el email
                if (!string.IsNullOrEmpty(usuarioActualizado.Email) && usuarioActualizado.Email != usuario.Email)
                {
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuarioActualizado.Email && u.IDUsuario != id))
                    {
                        Console.WriteLine("ERROR: Email ya en uso");
                        return BadRequest("El email ya está en uso por otro usuario");
                    }
                }

                // LOG: Estado actual antes de actualizar
                Console.WriteLine($"Datos actuales - Nombre: {usuario.Nombre}, Apellido: {usuario.Apellido}, Email: {usuario.Email}");

                // Actualizar solo los campos que vienen en el DTO (campos opcionales)
                if (!string.IsNullOrEmpty(usuarioActualizado.Nombre))
                {
                    usuario.Nombre = usuarioActualizado.Nombre;
                    Console.WriteLine($"Nombre actualizado a: {usuarioActualizado.Nombre}");
                }

                if (!string.IsNullOrEmpty(usuarioActualizado.Apellido))
                {
                    usuario.Apellido = usuarioActualizado.Apellido;
                    Console.WriteLine($"Apellido actualizado a: {usuarioActualizado.Apellido}");
                }

                if (!string.IsNullOrEmpty(usuarioActualizado.Email))
                {
                    usuario.Email = usuarioActualizado.Email;
                    Console.WriteLine($"Email actualizado a: {usuarioActualizado.Email}");
                }

                // Telefono puede ser null (se permite borrar el teléfono)
                if (usuarioActualizado.Telefono != null)
                {
                    usuario.Telefono = usuarioActualizado.Telefono;
                    Console.WriteLine($"Telefono actualizado a: {usuarioActualizado.Telefono}");
                }

                // Si se proporciona una nueva contraseña, actualizarla
                if (!string.IsNullOrEmpty(usuarioActualizado.ContraseñaString))
                {
                    Console.WriteLine("Actualizando contraseña...");
                    usuario.Contraseña = HashPassword(usuarioActualizado.ContraseñaString);
                    Console.WriteLine("Contraseña actualizada");
                }

                // Actualizar foto si se proporciona
                if (usuarioActualizado.Foto != null && usuarioActualizado.Foto.Length > 0)
                {
                    Console.WriteLine($"Actualizando foto: {usuarioActualizado.Foto.Length} bytes");
                    usuario.Foto = usuarioActualizado.Foto;
                    Console.WriteLine("Foto actualizada");
                }
                else
                {
                    Console.WriteLine("No se proporcionó nueva foto, manteniendo la actual");
                }

                Console.WriteLine("Guardando cambios en la base de datos...");
                await _context.SaveChangesAsync();
                Console.WriteLine("Cambios guardados exitosamente");
                Console.WriteLine("Usuario actualizado, preparando respuesta...");
                Console.WriteLine($"Foto en respuesta: {(usuario.Foto != null ? $"{usuario.Foto.Length} bytes" : "null")}");

                // No devolver la contraseña
                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                if (usuario.NivelUsuario == null)
                {
                    usuario.NivelUsuario = await _context.NivelesUsuario.FindAsync(usuario.IDNivel);
                }
                Console.WriteLine("=== FIN UpdateProfile API (ÉXITO) ===");
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en UpdateProfile API: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                Console.WriteLine($"Tipo de excepción: {ex.GetType().Name}");
                return StatusCode(500, new { error = "Error interno al actualizar el perfil" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            try
            {
                // Validar si el email ya existe
                if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                {
                    return BadRequest("El email ya está registrado");
                }

                // Validar y establecer IDNivel por defecto si es necesario
                if (usuario.IDNivel == 0)
                {
                    usuario.IDNivel = 1; // Cliente por defecto
                }

                // Verificar que el IDNivel exista en la base de datos
                var nivelExiste = await _context.NivelesUsuario.AnyAsync(n => n.IDNivel == usuario.IDNivel);
                if (!nivelExiste)
                {
                    return BadRequest($"El nivel de usuario con ID {usuario.IDNivel} no existe");
                }

                // Encriptar la contraseña antes de guardar
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

                // No devolver la contraseña en la respuesta
                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                return CreatedAtAction("GetUsuario", new { id = usuario.IDUsuario }, usuario);
            }
            catch (Exception ex)
            {
                // Log del error completo
                Console.WriteLine($"Error completo: {ex}");

                // En desarrollo, devolver detalles del error
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

        private byte[] HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
        {
            try
            {
                // Buscar usuario por email
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    return NotFound("Usuario no encontrado");
                }

                // Hashear la nueva contraseña
                usuario.Contraseña = HashPassword(request.NewPassword);

                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok("Contraseña actualizada correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la contraseña: {ex.Message}");
            }
        }

        // Clase para el request de actualización de contraseña
        public class UpdatePasswordRequest
        {
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class UsuarioViewModel
        {
            public int IDUsuario { get; set; }
            public string Email { get; set; }
            public string Telefono { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string NivelDescripcion { get; set; }
            public string Foto { get; set; }
        }
    }
}