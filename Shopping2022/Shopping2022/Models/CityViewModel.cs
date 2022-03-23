using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Models
{
    public class CityViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Ciudad")]
        public string Name { get; set; }

        public int StateId { get; set; }
    }
}
