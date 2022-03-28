using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;

namespace Shopping2022.Helpers
{
    public class ComboHelper : ICombosHelper
    {
        private readonly DataContext _context;

        public ComboHelper(DataContext context)
        {
            this._context = context;
        }

        public async Task<IEnumerable<SelectListItem>> GetComboCategoriesAsync()
        {
            var list = await _context.Categories.Select(c => new SelectListItem 
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
            var list = await _context.Countries.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
            }).OrderBy(c => c.Text).ToListAsync();

            list.Insert(0, new SelectListItem 
            { 
               Text = "[Selecciones un Pais.....]",
               Value   = "0"
            });

            return list;  
        }

        public async Task<IEnumerable<SelectListItem>> GetComboStatesByIdAsync(int countryId)
        {
            var list = await _context.States.Where(s => s.Id == countryId).Select(c => new SelectListItem 
            {
                Text =c.Name,
                Value= c.Id.ToString(),
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
            var list = await _context.Cities.Where(c => c.Id == stateId).Select(c => new SelectListItem 
            {
              Text=c.Name,
              Value = c.Id.ToString(),
            }).OrderBy(c => c.Text).ToListAsync();

            list.Insert(0, new SelectListItem 
            { 
                Text = "Selecciones una Ciudad.....",
                Value ="0"
            
            });

            return list;
        }               

      
    }
}
