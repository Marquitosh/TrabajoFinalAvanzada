using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.DTOs
{
    public class TipoServicioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El tiempo estimado es obligatorio.")]
        [Range(1, 1440, ErrorMessage = "El tiempo estimado debe estar entre 1 y 1440 minutos.")] // 1 min a 24 horas
        public int TiempoEstimado { get; set; } // En minutos

        public string? Descripcion { get; set; }
    }
}