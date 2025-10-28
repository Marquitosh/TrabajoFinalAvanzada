using System.Text.Json;

namespace AvanzadaWeb.ViewModels
{
    public class ScheduleAppointmentViewModel
    {
        // 1. Servicios que el usuario está reservando
        public List<TipoServicioViewModel> ServiciosSeleccionados { get; set; } = new List<TipoServicioViewModel>();

        // 2. Días pre-calculados desde la API
        public List<DiaDisponibleViewModel> DiasDisponibles { get; set; } = new List<DiaDisponibleViewModel>();

        // 3. Vehículos del usuario (NUEVO)
        public List<VehiculoViewModel> VehiculosUsuario { get; set; } = new List<VehiculoViewModel>();

        // --- Propiedades Calculadas para la Vista ---

        public int DuracionNuevoTurnoMinutos => ServiciosSeleccionados.Sum(s => s.TiempoEstimado);

        public decimal TotalAPagar => ServiciosSeleccionados.Sum(s => s.Precio);

        // --- JSON para JavaScript ---

        public string DiasDisponiblesJson => JsonSerializer.Serialize(DiasDisponibles, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}