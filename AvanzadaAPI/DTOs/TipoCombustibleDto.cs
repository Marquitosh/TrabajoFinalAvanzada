using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.DTOs
{
    public class TipoCombustibleDto
    {
        // El ID no se incluye aquí, se pasa por la ruta en PUT/DELETE
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50, ErrorMessage = "La descripción no puede exceder los 50 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;
    }
}