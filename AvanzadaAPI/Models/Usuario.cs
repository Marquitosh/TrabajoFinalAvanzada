using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvanzadaAPI.Models
{
    public class Usuario
    {
        [Key]
        public int IDUsuario { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [Required]
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [JsonIgnore]
        public byte[] Contraseña { get; set; } = new byte[0];

        // Propiedad para recibir la contraseña en texto plano
        [NotMapped]
        [JsonPropertyName("contraseñaString")]
        public string ContraseñaString { get; set; } = string.Empty;

        [ForeignKey("NivelUsuario")]
        public int IDNivel { get; set; } = 1; // Valor por defecto: Cliente

        public NivelUsuario? NivelUsuario { get; set; }

        public byte[]? Foto { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [JsonIgnore]
        public ICollection<Vehiculo>? Vehiculos { get; set; }

        [JsonIgnore]
        public ICollection<Turno>? Turnos { get; set; }

        [JsonIgnore]
        public Cliente? Cliente { get; set; }
    }
}