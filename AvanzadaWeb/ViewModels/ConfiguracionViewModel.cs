using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AvanzadaWeb.ViewModels
{
    // Usaremos directamente una estructura similar al DTO de la API
    // para simplificar el binding en el formulario.
    public class ConfiguracionViewModel
    {
        [JsonPropertyName("idHorario")] // Para asegurar el mapeo desde la API
        public int IdHorario { get; set; }

        [Display(Name = "Lunes - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("lunesInicio")]
        public string? LunesInicio { get; set; }

        [Display(Name = "Lunes - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("lunesFin")]
        public string? LunesFin { get; set; }

        [Display(Name = "Martes - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("martesInicio")] // etc.
        public string? MartesInicio { get; set; }

        [Display(Name = "Martes - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("martesFin")]
        public string? MartesFin { get; set; }

        [Display(Name = "Miércoles - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("miercolesInicio")]
        public string? MiercolesInicio { get; set; }

        [Display(Name = "Miércoles - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("miercolesFin")]
        public string? MiercolesFin { get; set; }

        [Display(Name = "Jueves - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("juevesInicio")]
        public string? JuevesInicio { get; set; }

        [Display(Name = "Jueves - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("juevesFin")]
        public string? JuevesFin { get; set; }

        [Display(Name = "Viernes - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("viernesInicio")]
        public string? ViernesInicio { get; set; }

        [Display(Name = "Viernes - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("viernesFin")]
        public string? ViernesFin { get; set; }

        [Display(Name = "Sábado - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("sabadoInicio")]
        public string? SabadoInicio { get; set; }

        [Display(Name = "Sábado - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("sabadoFin")]
        public string? SabadoFin { get; set; }

        [Display(Name = "Domingo - Inicio")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("domingoInicio")]
        public string? DomingoInicio { get; set; }

        [Display(Name = "Domingo - Fin")]
        //[DataType(DataType.Time)]
        [JsonPropertyName("domingoFin")]
        public string? DomingoFin { get; set; }

        public List<TipoCombustibleViewModel> TiposCombustible { get; set; } = new List<TipoCombustibleViewModel>();
        public List<TipoServicioViewModel> TiposServicio { get; set; } = new List<TipoServicioViewModel>();
    }
}