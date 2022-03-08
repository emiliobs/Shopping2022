using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Data.Entities
{
    public class Country
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "País")]
        public string Name { get; set; }

        //Relation one
        public ICollection<State> States  { get; set; }

        [Display(Name = "Departamentos/Estados")]
        public int StateNumber => States == null ? 0 : States.Count;

    }
}
