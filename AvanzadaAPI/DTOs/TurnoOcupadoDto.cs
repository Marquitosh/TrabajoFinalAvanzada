namespace AvanzadaAPI.DTOs
{
    public class TurnoOcupadoDto
    {
        public DateTime FechaHoraInicio { get; set; }
        public int DuracionTotalMinutos { get; set; }
        // Calculamos la hora de fin para facilitar el consumo en JS
        public DateTime FechaHoraFin => FechaHoraInicio.AddMinutes(DuracionTotalMinutos);
    }
}