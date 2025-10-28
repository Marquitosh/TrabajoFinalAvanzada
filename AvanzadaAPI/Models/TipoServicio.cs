using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvanzadaAPI.Models
{
    [Table("TipoServicio")]
    public class TipoServicio
    {
        [Key]
        public int IdTipoServicio { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        [Required]
        public int TiempoEstimado { get; set; } // Minutos

        [Column(TypeName = "text")]
        public string? Descripcion { get; set; }

        // Propiedad de navegación: Un tipo de servicio puede estar en muchos servicios
        [JsonIgnore]
        public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}