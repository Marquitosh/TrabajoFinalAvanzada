namespace AvanzadaAPI.DTOs
{
    public class TurnoUsuarioDto
    {
        public int IdTurno { get; set; }
        public string Vehiculo { get; set; } = string.Empty; // Formato: "Marca Modelo (Patente)"
        public DateTime FechaHora { get; set; }
        public int DuracionTotal { get; set; } // Calculada en minutos
        public string Estado { get; set; } = string.Empty; // Descripción del estado
        public List<string> ServiciosNombres { get; set; } = new List<string>(); // Lista de nombres
        public string? Observaciones { get; set; }
    }
}