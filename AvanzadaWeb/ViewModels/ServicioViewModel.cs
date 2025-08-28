using System.ComponentModel.DataAnnotations;

namespace AvanzadaWeb.ViewModels
{
    public class ServicioViewModel
    {
        public int IDServicio { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre del servicio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "El tiempo estimado es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El tiempo estimado debe ser mayor a 0")]
        [Display(Name = "Tiempo estimado (minutos)")]
        public int TiempoEstimado { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }
}