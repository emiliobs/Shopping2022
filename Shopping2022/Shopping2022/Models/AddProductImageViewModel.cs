using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Models
{
    public class AddProductImageViewModel
    {
        public int ProductId { get; set; }

        [Display(Name = "Foto")]
        [Required(ErrorMessage = "El campo {0} es Obligatorio.")]
        public IFormFile ImageFile { get; set; }
    }
}
