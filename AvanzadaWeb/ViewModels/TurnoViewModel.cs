namespace AvanzadaWeb.ViewModels
{
    // Este ViewModel representa la ESTRUCTURA DE DATOS que la API DEVUELVE
    // para un Turno.
    public class TurnoViewModel
    {
        public int IDTurno { get; set; }
        public DateTime FechaHora { get; set; }
        public string? Observaciones { get; set; }

        public VehiculoViewModel Vehiculo { get; set; }
        public EstadoTurnoViewModel EstadoTurno { get; set; }
        public List<ServicioApiViewModel> Servicios { get; set; }
    }

    // ViewModel para el Vehículo anidado
    // (Asegúrate que VehiculoViewModel.cs coincida)
    /*
    public class VehiculoViewModel
    {
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string Patente { get; set; }
    }
    */

    // ViewModel para el Estado anidado
    public class EstadoTurnoViewModel
    {
        public string Descripcion { get; set; }
    }

    // ViewModel para el Servicio anidado
    public class ServicioApiViewModel
    {
        public TipoServicioViewModel TipoServicio { get; set; }
    }
}