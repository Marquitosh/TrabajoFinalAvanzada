using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvanzadaAPI.Models
{
    [Table("Turno")]
    public class Turno
    {
        [Key]
        public int IDTurno { get; set; }

        [ForeignKey("Usuario")]
        public int IDUsuario { get; set; }
        public virtual Usuario? Usuario { get; set; }

        [ForeignKey("Vehiculo")]
        public int IDVehiculo { get; set; }
        public virtual Vehiculo? Vehiculo { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [ForeignKey("EstadoTurno")]
        public int IDEstadoTurno { get; set; }
        public virtual EstadoTurno? EstadoTurno { get; set; }

        [Column(TypeName = "text")]
        public string? Observaciones { get; set; }

        // Propiedad de navegación: Un turno incluye múltiples servicios
        [JsonIgnore]
        public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}