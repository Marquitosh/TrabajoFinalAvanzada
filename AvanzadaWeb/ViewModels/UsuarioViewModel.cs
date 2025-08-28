namespace AvanzadaWeb.ViewModels
{
    public class UsuarioViewModel
    {
        public int IDUsuario { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Contraseña { get; set; }
        public int IDNivel { get; set; }
        public string Foto { get; set; }
        public string NivelDescripcion { get; set; }
    }
}