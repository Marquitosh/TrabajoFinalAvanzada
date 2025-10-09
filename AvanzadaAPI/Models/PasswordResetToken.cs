using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int IDToken { get; set; }

        [Required]
        public int IDUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime FechaExpiracion { get; set; }

        [Required]
        public bool Utilizado { get; set; } = false;

        [Required]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("IDUsuario")]
        public Usuario Usuario { get; set; } = null!;
    }
}