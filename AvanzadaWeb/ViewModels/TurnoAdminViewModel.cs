using System.Text.Json.Serialization; // <-- Añadir using

namespace AvanzadaWeb.ViewModels
{
    // Este ViewModel coincide EXACTAMENTE con el TurnoAdminDto de la API
    public class TurnoAdminViewModel
    {
        [JsonPropertyName("idTurno")]
        public int IdTurno { get; set; }

        [JsonPropertyName("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [JsonPropertyName("vehiculo")]
        public string Vehiculo { get; set; } = string.Empty;

        [JsonPropertyName("fechaHora")]
        public DateTime FechaHora { get; set; }

        [JsonPropertyName("duracionTotal")]
        public int DuracionTotal { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("serviciosNombres")]
        public List<string> ServiciosNombres { get; set; } = new List<string>();

        [JsonPropertyName("observaciones")]
        public string? Observaciones { get; set; }
    }
}