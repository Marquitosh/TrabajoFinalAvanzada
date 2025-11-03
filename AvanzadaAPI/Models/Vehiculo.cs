using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        public int IDVehiculo { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [StringLength(15)]
        public string Patente { get; set; }

        public int? IDCombustible { get; set; } // Nullable si se permite no especificar
        public string? Observaciones { get; set; }

        [Required]
        public int IDUsuario { get; set; }

        [Required]
        public int IDMarca { get; set; }

        [Required]
        public int IDModelo { get; set; }

        // --- Navegación ---
        [ForeignKey("IDUsuario")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("IDCombustible")]
        public virtual TipoCombustible TipoCombustible { get; set; }

        [ForeignKey("IDMarca")]
        public virtual Marca Marca { get; set; }

        [ForeignKey("IDModelo")]
        public virtual Modelo Modelo { get; set; }
    }
}