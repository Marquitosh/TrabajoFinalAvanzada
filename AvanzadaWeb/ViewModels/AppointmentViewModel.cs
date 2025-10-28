namespace AvanzadaWeb.ViewModels
{
    public class AppointmentServiceViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public int Tiempo { get; set; }
        public decimal Costo { get; set; }
    }

    public class AppointmentViewModel
    {
        public int IDTurno { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Vehiculo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = string.Empty;
        public int DuracionTotal { get; set; }
        public string Estado { get; set; } = string.Empty;
        public List<AppointmentServiceViewModel> Servicios { get; set; } = new();
        public string? Observaciones { get; set; }
    }
}