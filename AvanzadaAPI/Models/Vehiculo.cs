using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    public class Vehiculo
    {
        [Key]
        public int IDVehiculo { get; set; }

        [Required]
        [MaxLength(50)]
        public string Marca { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Modelo { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(15)]
        public string Patente { get; set; } = string.Empty;

        [ForeignKey("TipoCombustible")]
        public int IDCombustible { get; set; }
        public TipoCombustible? TipoCombustible { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("Usuario")]
        public int IDUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        public ICollection<Turno>? Turnos { get; set; }
    }
}
