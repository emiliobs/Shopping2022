using Microsoft.AspNetCore.Mvc.Rendering;
using Shopping2022.Data.Entities;

namespace Shopping2022.Helpers
{
    public interface ICombosHelper
    {
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync();
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync(IEnumerable<Category> filter);

        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();

        Task<IEnumerable<SelectListItem>> GetComboStatesByIdAsync(int countryId);

        Task<IEnumerable<SelectListItem>> GetComboCitiesByIdyAsync(int countryId);
    }
}
