using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvanzadaAPI.Models
{
    [Table("Servicio")]
    public class Servicio
    {
        [Key]
        public int IdServicio { get; set; }

        // Foreign Key a Turno
        [ForeignKey("Turno")]
        public int IdTurno { get; set; }

        // Foreign Key a TipoServicio (la definición)
        [ForeignKey("TipoServicio")]
        public int IdTipoServicio { get; set; }

        // Foreign Key a EstadoServicio (Pendiente, En Progreso, etc.)
        [ForeignKey("EstadoServicio")]
        public int IdEstadoServicio { get; set; }

        [Column(TypeName = "text")]
        public string? Observaciones { get; set; }

        // --- Propiedades de Navegación (ASEGÚRATE DE QUE NO ESTÉN DUPLICADAS) ---

        [JsonIgnore] // Evitar referencias circulares
        public virtual Turno? Turno { get; set; } // SOLO UNA VEZ

        public virtual TipoServicio? TipoServicio { get; set; } // SOLO UNA VEZ

        public virtual EstadoServicio? EstadoServicio { get; set; }
    }
}