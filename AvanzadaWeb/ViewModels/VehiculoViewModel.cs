using System.ComponentModel.DataAnnotations;

namespace AvanzadaWeb.ViewModels
{
    public class VehiculoViewModel
    {
        public int IDVehiculo { get; set; }

        [Required(ErrorMessage = "La marca es requerida")]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "El modelo es requerido")]
        public string Modelo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El año es requerido")]
        [Range(1990, 2030, ErrorMessage = "El año debe estar entre 1990 y 2030")]
        public int Year { get; set; }

        [Required(ErrorMessage = "La patente es requerida")]
        [StringLength(7, MinimumLength = 6, ErrorMessage = "La patente debe tener 6 o 7 caracteres")]
        public string Patente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de combustible es requerido")]
        public int IDCombustible { get; set; }

        public string? Observaciones { get; set; }

        public string? UsuarioNombre { get; set; }
        public string? CombustibleDescripcion { get; set; }
    }
}