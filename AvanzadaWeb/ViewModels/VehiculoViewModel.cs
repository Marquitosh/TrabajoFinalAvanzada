namespace AvanzadaWeb.ViewModels
{
    public class VehiculoViewModel
    {
        public int IDVehiculo { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Year { get; set; }
        public string Patente { get; set; }
        public int IDCombustible { get; set; }
        public string CombustibleDescripcion { get; set; }
        public string Observaciones { get; set; }
        public int IDUsuario { get; set; }
        public string UsuarioNombre { get; set; }
    }
}