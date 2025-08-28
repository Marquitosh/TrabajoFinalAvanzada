using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    public class Cliente
    {
        [Key]
        public int IDCliente { get; set; }

        [ForeignKey("Usuario")]
        public int IDUsuario { get; set; }
        public Usuario? Usuario { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(100)]
        public string? Localidad { get; set; }

        [MaxLength(100)]
        public string? Provincia { get; set; }

        public string? Observaciones { get; set; }
    }
}