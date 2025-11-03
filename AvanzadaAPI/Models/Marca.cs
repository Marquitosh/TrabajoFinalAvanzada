using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    [Table("Marcas")]
    public class Marca
    {
        [Key]
        public int IDMarca { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}