using AvanzadaAPI.Data;
using AvanzadaAPI.Models;
using Microsoft.AspNetCore.Identity.Data;
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

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.NivelUsuario)
                .FirstOrDefaultAsync(u => u.IDUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
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
                    NivelDescripcion = usuario.NivelUsuario.Descripcion,
                    Foto = usuario.Foto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.IDUsuario)
            {
                return BadRequest();
            }

            // Si se está actualizando la contraseña, encriptarla
            if (!string.IsNullOrEmpty(usuario.ContraseñaString))
            {
                usuario.Contraseña = HashPassword(usuario.ContraseñaString);
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Validar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                return BadRequest("El email ya está registrado");
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
            usuario.IDNivel = 1; // Por defecto, asignar como cliente

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // No devolver la contraseña en la respuesta
            usuario.Contraseña = Array.Empty<byte>();
            usuario.ContraseñaString = string.Empty;

            return CreatedAtAction("GetUsuario", new { id = usuario.IDUsuario }, usuario);
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

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IDUsuario == id);
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