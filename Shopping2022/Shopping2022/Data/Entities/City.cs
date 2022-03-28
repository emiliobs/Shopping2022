using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Shopping2022.Data.Entities
{
    public class City
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Display(Name = "Ciudad")]
        public string Name { get; set; }

        [JsonIgnore]
        public State State { get; set; }

        public ICollection<User> Users { get; set; }

    }
}
