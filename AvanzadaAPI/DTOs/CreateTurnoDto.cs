using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.DTOs
{
    public class CreateTurnoDto
    {
        [Required]
        public int IDUsuario { get; set; }

        [Required]
        public int IDVehiculo { get; set; }

        [Required]
        public DateTime FechaHora { get; set; }

        [Required]
        public int IDEstadoTurno { get; set; }

        [Required]
        public List<int> IdTipoServicios { get; set; } = new List<int>();
    }
}