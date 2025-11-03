using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AvanzadaWeb.ViewModels
{
    // Usaremos directamente una estructura similar al DTO de la API
    // para simplificar el binding en el formulario.
    public class ConfiguracionViewModel
    {
        [JsonPropertyName("idHorario")] // Para asegurar el mapeo desde la API
        public int IdHorario { get; set; }

        [Display(Name = "Lunes - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("lunesInicio")]
        public string? LunesInicio { get; set; }

        [Display(Name = "Lunes - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("lunesFin")]
        public string? LunesFin { get; set; }

        [Display(Name = "Martes - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("martesInicio")] // etc.
        public string? MartesInicio { get; set; }

        [Display(Name = "Martes - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("martesFin")]
        public string? MartesFin { get; set; }

        [Display(Name = "Miércoles - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("miercolesInicio")]
        public string? MiercolesInicio { get; set; }

        [Display(Name = "Miércoles - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("miercolesFin")]
        public string? MiercolesFin { get; set; }

        [Display(Name = "Jueves - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("juevesInicio")]
        public string? JuevesInicio { get; set; }

        [Display(Name = "Jueves - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("juevesFin")]
        public string? JuevesFin { get; set; }

        [Display(Name = "Viernes - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("viernesInicio")]
        public string? ViernesInicio { get; set; }

        [Display(Name = "Viernes - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("viernesFin")]
        public string? ViernesFin { get; set; }

        [Display(Name = "Sábado - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("sabadoInicio")]
        public string? SabadoInicio { get; set; }

        [Display(Name = "Sábado - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("sabadoFin")]
        public string? SabadoFin { get; set; }

        [Display(Name = "Domingo - Inicio")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("domingoInicio")]
        public string? DomingoInicio { get; set; }

        [Display(Name = "Domingo - Fin")]
        [DisplayFormat(DataFormatString = "{0}", ApplyFormatInEditMode = true)]
        [JsonPropertyName("domingoFin")]
        public string? DomingoFin { get; set; }

        public List<TipoCombustibleViewModel> TiposCombustible { get; set; } = new List<TipoCombustibleViewModel>();
        public List<TipoServicioViewModel> TiposServicio { get; set; } = new List<TipoServicioViewModel>();
        public List<MarcaViewModel> Marcas { get; set; } = new List<MarcaViewModel>();
        public List<ModeloViewModel> Modelos { get; set; } = new List<ModeloViewModel>();
        public IEnumerable<SelectListItem> MarcasList { get; set; } = new List<SelectListItem>();
    }
}