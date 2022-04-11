using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;

namespace Shopping2022.Helpers
{
    public class ComboHelper : ICombosHelper
    {
        private readonly DataContext _context;

        public ComboHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync()
        {
            List<SelectListItem> list = await _context.Categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            }).OrderBy(c => c.Text).ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "[Seleccione una Categoría.....]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCountriesAsync()
        {
            List<SelectListItem> list = await _context.Countries.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            }).OrderBy(c => c.Text).ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "[Selecciones un Pais.....]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboStatesByIdAsync(int countryId)
        {
            List<SelectListItem> list = await _context.States.Where(s => s.Id == countryId).Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            }).OrderBy(s => s.Text).ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "[Selecciones un Departamento / Estado.....]",
                Value = "0"
            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCitiesByIdyAsync(int stateId)
        {
            List<SelectListItem> list = await _context.Cities.Where(c => c.Id == stateId).Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            }).OrderBy(c => c.Text).ToListAsync();

            list.Insert(0, new SelectListItem
            {
                Text = "Selecciones una Ciudad.....",
                Value = "0"

            });

            return list;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync(IEnumerable<Category> filter)
        {
            List<Category> categories = await _context.Categories.ToListAsync();
            List<Category> categoryFilter = new();

            foreach (Category category in categories)
            {
                if (!filter.Any(c => c.Id == category.Id))
                {
                    categoryFilter.Add(category);
                }

            }

            List<SelectListItem> list = categoryFilter.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            }).OrderBy(c => c.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Seleccione una Categoría.....]",
                Value = "0",
            });

            return list;

        }
    }
}
