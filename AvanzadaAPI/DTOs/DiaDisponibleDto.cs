namespace AvanzadaAPI.DTOs
{
    // Representa un día en el selector (ej: "Lunes 3 de Noviembre")
    public class DiaDisponibleDto
    {
        // El texto a mostrar en el <option>
        public string Texto { get; set; } = string.Empty;

        // La lista de slots (horarios) disponibles para ESE día
        public List<HorarioDisponibleDto> Slots { get; set; } = new List<HorarioDisponibleDto>();
    }
}