using Microsoft.AspNetCore.Mvc.Rendering;

namespace Shopping2022.Helpers
{
    public interface ICombosHelper
    {
        Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync();

        Task<IEnumerable<SelectListItem>> GetComboCountriesAsync();

        Task<IEnumerable<SelectListItem>> GetComboStatesByIdAsync(int countryId);

        Task<IEnumerable<SelectListItem>> GetComboCitiesByIdyAsync(int countryId);
    }
}
