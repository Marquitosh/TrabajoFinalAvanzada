using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AvanzadaWeb.ViewModels
{
    public class UsuarioViewModel
    {
        public int IDUsuario { get; set; }

        // Quitamos NivelDescripcion como requerido ya que no se usa en la edición
        public string? NivelDescripcion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede tener más de 100 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede tener más de 20 caracteres")]
        [RegularExpression(@"^[0-9+\-\s()]+$", ErrorMessage = "El teléfono solo puede contener números, espacios, +, -, paréntesis")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Foto de Perfil")]
        public string? FotoBase64 { get; set; }

        [Display(Name = "Nivel de Usuario")]
        public int IDNivel { get; set; }

        [JsonIgnore]
        public string? Foto => FotoBase64;

        // Propiedades de solo lectura para mostrar información
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        // Clase para validaciones personalizadas adicionales
        public class ValidacionPersonalizada : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                // Aquí puedes implementar validaciones más complejas si es necesario
                return ValidationResult.Success;
            }
        }

        // Clase para manejar la respuesta de actualización
        public class UpdateProfileResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public UsuarioViewModel Usuario { get; set; }
        }


    }
}