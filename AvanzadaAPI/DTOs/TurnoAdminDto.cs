namespace AvanzadaAPI.DTOs
{
    // DTO específico para la vista de gestión de turnos del admin
    public class TurnoAdminDto
    {
        public int IdTurno { get; set; }
        public string Usuario { get; set; } = string.Empty; // Nombre completo del usuario
        public string Vehiculo { get; set; } = string.Empty; // Formato: "Marca Modelo (Patente)"
        public DateTime FechaHora { get; set; }
        public int DuracionTotal { get; set; } // Calculada en minutos
        public string Estado { get; set; } = string.Empty; // Descripción del estado
        public List<string> ServiciosNombres { get; set; } = new List<string>(); // Lista de nombres
        public string? Observaciones { get; set; }
    }
}