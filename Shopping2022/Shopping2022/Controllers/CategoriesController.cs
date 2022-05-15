using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Vereyon.Web;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CategoriesController(DataContext context, IFlashMessage flashMessage)
        {
            _context = context;
            this._flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Categories.Add(category);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un Categoría con el mismo nombre.");
                       
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category? category = await _context.Categories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Update(category);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe una Categoría con el mismo nombre.");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }

            }

            return View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category? category = await _context.Categories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            
            return category == null ? NotFound() : View(category);
        }

        [HttpPost, ActionName("Delete")]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {


            Category? category = await _context.Categories.FindAsync(id);
            _ = _context.Categories.Remove(category);
            _ = await _context.SaveChangesAsync();

            _flashMessage.Confirmation("Registro Borrado.");

            return RedirectToAction(nameof(Index));

            //Country country = await _context.Countries.FindAsync(id);
            //_context.Countries.Remove(country);
            //await _context.SaveChangesAsync();
            //return RedirectToAction(nameof(Index));
        }

    }
}
