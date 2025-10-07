
namespace AvanzadaWeb.ViewModels
{
    public class LogViewModel
    {
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Descripcion { get; set; }
        public string Nivel { get; set; }
        public string? IPAddress { get; set; }
    }

}