using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Helpers;
using Vereyon.Web;
using static Shopping2022.Helpers.ModalHelper;

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
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories
                             .Include(c => c.productCategories)
                             .ToListAsync());
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

        [NoDirectAccess]
        [HttpGet]
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Category());
            }
            else
            {
                Category category = await _context.Categories.FindAsync(id);
                return category is null ? NotFound() : View(category);
            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == 0) //Insert
                    {
                        _ = _context.Add(category);
                        _ = await _context.SaveChangesAsync();
                        _flashMessage.Info("Registro Creado.");
                    }
                    else//Update
                    {
                        _ = _context.Update(category);
                        _ = await _context.SaveChangesAsync();
                        _flashMessage.Info("Registo Actualizado");
                    }
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe una Categoría con el mismo nombre.");
                    }
                    else
                    {
                        return View(category);
                    }

                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                    return View(category);
                }

                return Json(new { isValid = true, html = ModalHelper.RenderRazorViewToString(this, "_ViewAll", _context.Categories.Include(c => c.productCategories).ToList()) });
            }

            return Json(new { isValid = false, html = ModalHelper.RenderRazorViewToString(this, "AddOrEdit", category) });
        }

        [NoDirectAccess]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            try
            {
                _ = _context.Categories.Remove(category);
                _ = await _context.SaveChangesAsync();
                _flashMessage.Info("Registro Borrado.");
            }
            catch
            {

                _flashMessage.Danger("No se puede borrar la categoría porque tiene registros relacionados.");
            }

            return RedirectToAction(nameof(Index));
        }



    }
}
