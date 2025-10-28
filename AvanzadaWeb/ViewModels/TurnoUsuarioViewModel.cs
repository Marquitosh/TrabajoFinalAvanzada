namespace AvanzadaWeb.ViewModels
{
    // Este ViewModel coincide EXACTAMENTE con el DTO de la API
    public class TurnoUsuarioViewModel
    {
        // Usa JsonPropertyName por seguridad, aunque ahora los nombres coincidan
        [System.Text.Json.Serialization.JsonPropertyName("idTurno")]
        public int IdTurno { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("vehiculo")]
        public string Vehiculo { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("fechaHora")]
        public DateTime FechaHora { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("duracionTotal")]
        public int DuracionTotal { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("serviciosNombres")]
        public List<string> ServiciosNombres { get; set; } = new List<string>();

        [System.Text.Json.Serialization.JsonPropertyName("observaciones")]
        public string? Observaciones { get; set; }
    }
}