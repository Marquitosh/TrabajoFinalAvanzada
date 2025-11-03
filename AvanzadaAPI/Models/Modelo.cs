using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvanzadaAPI.Models
{
    [Table("Modelos")]
    public class Modelo
    {
        [Key]
        public int IDModelo { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public int IDMarca { get; set; }

        [ForeignKey("IDMarca")]
        public virtual Marca Marca { get; set; }
    }
}