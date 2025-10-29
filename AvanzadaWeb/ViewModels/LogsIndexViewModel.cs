// AvanzadaWeb/ViewModels/LogsIndexViewModel.cs
namespace AvanzadaWeb.ViewModels
{
    public class LogsIndexViewModel
    {
        public IEnumerable<LogViewModel> Logs { get; set; } = new List<LogViewModel>();
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Nivel { get; set; }
        public string? Usuario { get; set; }
    }
}