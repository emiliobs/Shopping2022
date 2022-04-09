using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Helpers;
using Shopping2022.Models;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;

        public ProductsController(DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Products
             .Include(p => p.ProductImages)
             .Include(p => p.ProductCategories)
             .ThenInclude(pc => pc.Category)
             .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            CreateProductViewModel model = new CreateProductViewModel
            {
                Categories = await _combosHelper.GetComboCategoriesAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;
                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");

                }

                Product product = new()
                {
                    Description = model.Description,
                    Name = model.Name,
                    Price = model.Price,
                    Stock = model.Stock,

                    //<aqui le agrego al menos una categoria al producto nuevo por 1 ves.
                    ProductCategories = new List<ProductCategory>
                {
                    new ProductCategory
                    {
                        Category = await _context.Categories.FindAsync(model.CategoryId),
                    }
                 }
                };

                if (imageId != Guid.Empty)
                {
                    product.ProductImages = new List<ProductImage>
                    {
                      new ProductImage { ImageId = imageId}
                    };
                }

                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            model.Categories = await _combosHelper.GetComboCategoriesAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Product product = await _context.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            EditProductViewModel model = new EditProductViewModel
            {
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }


                try
                {
                    Product product = await _context.Products.FindAsync(model.Id);

                    product.Description = model.Description;
                    product.Name = model.Name;
                    product.Price = model.Price;
                    product.Stock = model.Stock;

                    _context.UpdateRange(product);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Ya existe un producto con el mismo nombre.");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            

            return View(model);
        }
    }
}
