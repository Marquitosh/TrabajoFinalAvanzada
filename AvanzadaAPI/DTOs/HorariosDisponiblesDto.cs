using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.DTOs
{
    public class HorariosDisponiblesDto
    {
        // Añadimos el ID por si necesitas identificar el registro, aunque asumimos que solo hay uno.
        public int IdHorario { get; set; }

        // Usamos string para facilitar la vinculación con inputs de tipo "time" en HTML.
        // Se aplicará validación para asegurar el formato HH:mm.
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? LunesInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? LunesFin { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? MartesInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? MartesFin { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? MiercolesInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? MiercolesFin { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? JuevesInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? JuevesFin { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? ViernesInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? ViernesFin { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? SabadoInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? SabadoFin { get; set; }

        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? DomingoInicio { get; set; }
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato inválido (HH:mm)")]
        public string? DomingoFin { get; set; }
    }
}