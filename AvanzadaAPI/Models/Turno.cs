using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    public class Turno
    {
        [Key]
        public int IDTurno { get; set; }

        [ForeignKey("Usuario")]
        public int IDUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [ForeignKey("Vehiculo")]
        public int IDVehiculo { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [ForeignKey("Servicio")]
        public int IDServicio { get; set; }
        public Servicio? Servicio { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [ForeignKey("EstadoTurno")]
        public int IDEstadoTurno { get; set; }
        public EstadoTurno? EstadoTurno { get; set; }

        public string? Observaciones { get; set; }
    }
}
