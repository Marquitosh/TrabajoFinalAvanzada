using System.ComponentModel.DataAnnotations;

namespace AvanzadaAPI.DTOs
{
    // DTO solo con los datos necesarios para CREAR un vehículo
    public class VehiculoCreateDto
    {
        [Required]
        public int Year { get; set; }

        [Required]
        [StringLength(15)]
        public string Patente { get; set; }

        [Required]
        public int IDCombustible { get; set; }

        public string? Observaciones { get; set; }

        [Required]
        public int IDUsuario { get; set; }

        [Required]
        public int IDMarca { get; set; }

        [Required]
        public int IDModelo { get; set; }
    }
}