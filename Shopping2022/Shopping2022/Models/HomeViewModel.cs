using Shopping2022.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Shopping2022.Models
{
    public class HomeViewModel
    {
        public ICollection<Product> Products { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Cantidad")]
        public float Quantity { get; set; }
    }
}
