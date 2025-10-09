using System.ComponentModel.DataAnnotations;

namespace AvanzadaWeb.ViewModels
{
    public class PasswordRecoveryRequest
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }
}