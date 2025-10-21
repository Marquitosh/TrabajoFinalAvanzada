namespace AvanzadaWeb.Models
{
    public class SessionUser
    {
        public int IDUsuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string RolNombre { get; set; } = string.Empty;
        public string? Foto { get; set; }
        public string UrlDefault { get; set; } = "/Usuarios/Profile";
    }
}