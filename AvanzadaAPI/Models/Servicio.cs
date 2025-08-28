using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    public class Servicio
    {
        [Key]
        public int IDServicio { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        [Required]
        public int TiempoEstimado { get; set; } // En minutos

        public string? Descripcion { get; set; }

        public ICollection<Turno>? Turnos { get; set; }
    }
}
