using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.Models
{
    public class EstadoTurno
    {
        [Key]
        public int IDEstadoTurno { get; set; }

        [Required]
        [MaxLength(20)]
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Turno>? Turnos { get; set; }
    }
}
