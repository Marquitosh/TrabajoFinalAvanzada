namespace AvanzadaWeb.Models
{
    public class SessionUser
    {
        public int IDUsuario { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NivelUsuario { get; set; }
        public string Foto { get; set; }
    }
}