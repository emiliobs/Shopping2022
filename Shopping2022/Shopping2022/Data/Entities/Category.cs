using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Data.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Categoría")]
        public string Name { get; set; }
    }
}
