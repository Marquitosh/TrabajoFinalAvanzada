using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvanzadaAPI.Models
{
    [Table("EstadoServicio")]
    public class EstadoServicio
    {
        [Key]
        public int IdEstadoServicio { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        // Propiedad de navegación: Un estado puede estar en muchos servicios
        [JsonIgnore]
        public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}