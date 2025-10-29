// AvanzadaWeb/ViewModels/LogViewModel.cs
namespace AvanzadaWeb.ViewModels
{
    public class LogViewModel
    {
        public int IDLog { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Nivel { get; set; } = string.Empty;
        public string? IPAddress { get; set; }
    }
}