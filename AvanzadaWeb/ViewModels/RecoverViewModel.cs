using System.ComponentModel.DataAnnotations;

namespace AvanzadaWeb.ViewModels
{
    public class RecoverViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        [Display(Name = "Código de verificación")]
        public string VerificationCode { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        public bool CodeSent { get; set; }
    }
}