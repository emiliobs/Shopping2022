using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Helpers;
using Shopping2022.Models;
using Vereyon.Web;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IFlashMessage _flashMessage;

        public ProductsController(DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper, IFlashMessage flashMessage)
        {
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _flashMessage = flashMessage;
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
            CreateProductViewModel model = new()
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
                    _ = _context.Add(product);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un producto con el mismo nombre.");
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

            EditProductViewModel model = new()
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
                _ = await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                {
                    _flashMessage.Danger("Ya existe un producto con el mismo nombre.");
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



            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                _ = NotFound();
            }

            Product product = await _context.Products
                                        .Include(p => p.ProductImages)
                                        .Include(p => p.ProductCategories)
                                        .ThenInclude(pc => pc.Category)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            return product is null ? NotFound() : View(product);
        }

        [HttpGet]
        public async Task<IActionResult> AddImage(int? id)
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

            AddProductImageViewModel model = new()
            {
                ProductId = product.Id,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddImage(AddProductImageViewModel model)
        {

            if (ModelState.IsValid)
            {
                Guid imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                Product product = await _context.Products.FindAsync(model.ProductId);

                ProductImage productImage = new()
                {
                    Product = product,
                    ImageId = imageId,
                };

                try
                {
                    _ = _context.Add(productImage);
                    _ = await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { product.Id });
                }
                catch (Exception ex)
                {

                    _flashMessage.Danger(ex.Message);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ProductImage productImage = await _context.ProductImages.Include(pi => pi.Product).FirstOrDefaultAsync(pi => pi.Id == id);

            if (productImage == null)
            {
                return NotFound();
            }

            await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            _ = _context.ProductImages.Remove(productImage);
            _ = await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = productImage.Product.Id });

        }

        [HttpGet]
        public async Task<IActionResult> AddCategory(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            IEnumerable<Category> categories = product.ProductCategories.Select(pc => new Category
            {
                Id = pc.Category.Id,
                Name = pc.Category.Name,
            });

            AddCategoryProductViewModel model = new()
            {
                ProductId = product.Id,
                Categories = await _combosHelper.GetComboCategoriesAsync(categories),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(AddCategoryProductViewModel model)
        {

            Product product = await _context.Products
             .Include(p => p.ProductCategories)
             .ThenInclude(pc => pc.Category)
             .FirstOrDefaultAsync(p => p.Id == model.ProductId);

            if (ModelState.IsValid)
            {
                if (product is null)
                {
                    return NotFound();
                }

                ProductCategory productCategory = new()
                {
                    Category = await _context.Categories.FindAsync(model.CategoryId),
                    Product = product,
                };

                try
                {
                    _ = _context.Add(productCategory);
                    _ = await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { id = product.Id });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("El Prodycto ya tiene esa Categoría.");
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

            IEnumerable<Category> categories = product.ProductCategories.Select(pc => new Category
            {
                Id = pc.Category.Id,
                Name = pc.Category.Name,
            });

            model.Categories = await _combosHelper.GetComboCategoriesAsync(categories);

            return View(model);
        }

        public async Task<IActionResult> DeleteCategory(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            ProductCategory productCategory = await _context.ProductCategories.Include(pc => pc.Product).FirstOrDefaultAsync(pc => pc.Id == id);

            if (productCategory is null)
            {
                return NotFound();
            }

            _ = _context.ProductCategories.Remove(productCategory);
            _ = await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = productCategory.Product.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Product product = await _context.Products
                .Include(p => p.ProductCategories)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product is null ? NotFound() : View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Product model)
        {
            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            _ = _context.Products.Remove(product);
            _ = await _context.SaveChangesAsync();

            foreach (ProductImage productImage in product.ProductImages)
            {
                await _blobHelper.DeleteBlobAsync(productImage.ImageId, "products");
            }

            _flashMessage.Confirmation("Registro Borrado");


            return RedirectToAction(nameof(Index));
        }

    }
}
