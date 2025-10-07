using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.Models
{
    public class Log
    {
        [Key]
        public int IDLog { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(100)]
        public string Usuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Accion { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Nivel { get; set; } = "Info";

        [MaxLength(50)]
        public string? IPAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }
    }
}