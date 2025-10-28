using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AvanzadaWeb.ViewModels
{
    public class TipoCombustibleViewModel
    {
        [JsonPropertyName("idCombustible")]
        public int IdCombustible { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(50)]
        [Display(Name = "Descripción")]
        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}