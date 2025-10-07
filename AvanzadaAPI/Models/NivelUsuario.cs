using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AvanzadaAPI.Models
{
    public class NivelUsuario
    {
        [Key]
        public int IDNivel { get; set; }

        [Required]
        [MaxLength(50)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RolNombre { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? UrlDefault { get; set; }

        [JsonIgnore]
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}