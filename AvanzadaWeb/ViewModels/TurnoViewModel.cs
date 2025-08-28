namespace AvanzadaWeb.ViewModels
{
    public class TurnoViewModel
    {
        public int IDTurno { get; set; }
        public int IDUsuario { get; set; }
        public string UsuarioNombre { get; set; }
        public int IDVehiculo { get; set; }
        public string VehiculoInfo { get; set; }
        public int IDServicio { get; set; }
        public string ServicioNombre { get; set; }
        public DateTime FechaHora { get; set; }
        public int IDEstadoTurno { get; set; }
        public string EstadoDescripcion { get; set; }
        public string Observaciones { get; set; }
    }
}