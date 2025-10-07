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
                    return Unauthorized("Credenciales inválidas");
                }

                // DEBUG
                Console.WriteLine($"IDNivel desde BD: {usuario.IDNivel}");

                var response = new UsuarioLoginDto
                {
                    IDUsuario = usuario.IDUsuario,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    IDNivel = usuario.IDNivel, // Esto debería ser 2
                    Foto = usuario.Foto,
                    RolNombre = usuario.NivelUsuario?.RolNombre
                };

                Console.WriteLine($"IDNivel en DTO: {response.IDNivel}");

                return Ok(response);
            }
            catch (Exception ex)
            {
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

                if (!string.IsNullOrEmpty(usuarioActualizado.Nombre))
                {
                    usuario.Nombre = usuarioActualizado.Nombre;
                }

                if (!string.IsNullOrEmpty(usuarioActualizado.Apellido))
                {
                    usuario.Apellido = usuarioActualizado.Apellido;
                }

                if (!string.IsNullOrEmpty(usuarioActualizado.Email))
                {
                    usuario.Email = usuarioActualizado.Email;
                }

                if (usuarioActualizado.Telefono != null)
                {
                    usuario.Telefono = usuarioActualizado.Telefono;
                }

                if (!string.IsNullOrEmpty(usuarioActualizado.ContraseñaString))
                {
                    usuario.Contraseña = HashPassword(usuarioActualizado.ContraseñaString);
                }

                if (usuarioActualizado.Foto != null && usuarioActualizado.Foto.Length > 0)
                {
                    usuario.Foto = usuarioActualizado.Foto;
                }

                await _context.SaveChangesAsync();

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

                usuario.Contraseña = Array.Empty<byte>();
                usuario.ContraseñaString = string.Empty;

                return CreatedAtAction("GetUsuario", new { id = usuario.IDUsuario }, usuario);
            }
            catch (Exception ex)
            {

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

                // Actualizar solo los campos permitidos
                if (usuarioActualizado.IDNivel.HasValue)
                {
                    // Verificar que el nivel existe
                    var nivelExiste = await _context.NivelesUsuario.AnyAsync(n => n.IDNivel == usuarioActualizado.IDNivel.Value);
                    if (!nivelExiste)
                    {
                        return BadRequest("El nivel de usuario especificado no existe");
                    }
                    usuario.IDNivel = usuarioActualizado.IDNivel.Value;
                }

                await _context.SaveChangesAsync();

                // No devolver la contraseña
                usuario.Contraseña = Array.Empty<byte>();

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        public class UpdateUsuarioDto
        {
            public int? IDNivel { get; set; }
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

                // Verificar que el nuevo rol existe
                var rolExiste = await _context.NivelesUsuario.AnyAsync(n => n.IDNivel == actualizarRolDto.IDNivel);
                if (!rolExiste)
                {
                    return BadRequest("El rol especificado no existe");
                }

                usuario.IDNivel = actualizarRolDto.IDNivel;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Rol actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private bool VerifyPassword(string password, byte[] storedHash)
        {
            using var sha256 = SHA256.Create();
            var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return passwordHash.SequenceEqual(storedHash);
        }

        public class ActualizarRolDto
        {
            public int IDNivel { get; set; }
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