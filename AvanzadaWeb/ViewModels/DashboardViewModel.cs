using AvanzadaWeb.Models;

namespace AvanzadaWeb.ViewModels
{
    public class DashboardViewModel
    {
        public SessionUser? User { get; set; }
        public int VehiculosCount { get; set; }
        public int TurnosCount { get; set; }
        public string? ProximoTurno { get; set; }
        public string? ProximoService { get; set; }
        public DateTime? ProximoTurnoFecha { get; set; }
        public TimeSpan? ProximoTurnoHora { get; set; }
    }

    public class ProximoTurnoDto
    {
        public DateTime? Fecha { get; set; }
        public TimeSpan? Hora { get; set; }
        public string? Descripcion { get; set; }
        public DateTime? FechaHora { get; set; }
    }

    public class VehiculoCountDto
    {
        public int Count { get; set; }
    }

    public class TurnoCountDto
    {
        public int Count { get; set; }
    }

}