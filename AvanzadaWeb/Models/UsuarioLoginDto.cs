using Microsoft.AspNetCore.Mvc;

namespace AvanzadaWeb.Models
{
    public class UsuarioLoginDto
    {
        public int IDUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int IDNivel { get; set; }
        public byte[]? Foto { get; set; }
        public string? RolNombre { get; set; }
    }
}
