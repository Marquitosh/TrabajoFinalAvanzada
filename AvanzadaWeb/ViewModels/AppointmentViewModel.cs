namespace AvanzadaWeb.ViewModels
{
    public class AppointmentServiceViewModel
    {
        public string Nombre { get; set; } = "";
        public int Tiempo { get; set; }
        public decimal Costo { get; set; }
    }

    public class AppointmentViewModel
    {
        public string Usuario { get; set; } = "";
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = "";
        public List<AppointmentServiceViewModel> Servicios { get; set; } = new();

        // Helpers para no recalcular en la vista
        public int TiempoTotal => Servicios.Sum(s => s.Tiempo);
        public decimal CostoTotal => Servicios.Sum(s => s.Costo);
    }
}
