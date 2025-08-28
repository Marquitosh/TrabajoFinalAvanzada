using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.Models
{
    public class TipoCombustible
    {
        [Key]
        public int IDCombustible { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Vehiculo>? Vehiculos { get; set; }
    }
}
