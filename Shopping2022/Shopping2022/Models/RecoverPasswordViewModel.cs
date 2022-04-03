using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Models
{
    public class RecoverPasswordViewModel
    {
        [Display(Name = "Email")]
        [Required(ErrorMessage = "El campo {0} es Obligatorio.")]
        [EmailAddress(ErrorMessage = "Debes ingresar un Correo valido.")]
        public string Email { get; set; }
    }
}
