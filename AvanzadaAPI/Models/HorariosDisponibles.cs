using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    [Table("HorariosDisponibles")]
    public class HorariosDisponibles
    {
        [Key]
        public int IdHorario { get; set; }
        public TimeSpan? LunesInicio { get; set; }
        public TimeSpan? LunesFin { get; set; }
        public TimeSpan? MartesInicio { get; set; }
        public TimeSpan? MartesFin { get; set; }
        public TimeSpan? MiercolesInicio { get; set; }
        public TimeSpan? MiercolesFin { get; set; }
        public TimeSpan? JuevesInicio { get; set; }
        public TimeSpan? JuevesFin { get; set; }
        public TimeSpan? ViernesInicio { get; set; }
        public TimeSpan? ViernesFin { get; set; }
        public TimeSpan? SabadoInicio { get; set; }
        public TimeSpan? SabadoFin { get; set; }
        public TimeSpan? DomingoInicio { get; set; }
        public TimeSpan? DomingoFin { get; set; }
    }
}