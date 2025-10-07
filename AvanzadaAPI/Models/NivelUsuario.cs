using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.Models
{
    public class NivelUsuario
    {
        [Key]
        public int IDNivel { get; set; }

        [Required]
        [MaxLength(20)]
        public string Descripcion { get; set; } = string.Empty;
        public string URL { get; set; }

        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
