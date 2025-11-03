using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.DTOs
{
    public class ModeloCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de marca inválido.")]
        public int IDMarca { get; set; }
    }
}