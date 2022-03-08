using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Data.Entities
{
    public class State
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Departamento/Estado")]
        public string Name { get; set; }

        //relations side one
        public Country Country { get; set; }

        public ICollection<City> Cities { get; set; }

        [Display(Name = "Cities")]
        public int CitiesNumber => Cities == null ? 0 : Cities.Count;
    }
}
