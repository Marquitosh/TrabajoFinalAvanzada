namespace AvanzadaAPI.DTOs
{
    // Representa un único slot de hora (ej: "09:30")
    public class HorarioDisponibleDto
    {
        // El texto a mostrar en el botón (ej: "09:30")
        public string Hora { get; set; } = string.Empty;

        // La fecha y hora completas en formato ISO para el POST (ej: "2025-10-28T09:30:00")
        public DateTime FechaHoraISO { get; set; }
    }
}