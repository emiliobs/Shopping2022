using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Helpers;
using Shopping2022.Models;
using System.Diagnostics;

namespace Shopping2022.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public HomeController(ILogger<HomeController> logger, DataContext context, IUserHelper userHelper)
        {
            _logger = logger;
            _context = context;
            _userHelper = userHelper;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products
                                                   .Include(p => p.ProductImages)
                                                   .Include(p => p.ProductCategories)
                                                   .OrderBy(p => p.Description)
                                                   .ToListAsync();

            HomeViewModel model = new()
            {
                Products = products
            };

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user != null)
            {
                model.Quantity = await _context.TemporalSales.Where(ts => ts.User.Id == user.Id)
                                                             .SumAsync(ts => ts.Quantity);
            }


            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Product product = await _context.Products.Include(p => p.ProductImages)
                                                 .Include(p => p.ProductCategories)
                                                 .ThenInclude(pc => pc.Category)
                                                 .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return NotFound();
            }

            string categories = string.Empty;
            foreach (ProductCategory category in product.ProductCategories)
            {
                categories += $"{category.Category.Name}, ";
            }

            categories = categories[..^2];

            AddProductToCartViewModel model = new()
            {
                Categories = categories,
                Description = product.Description,
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                ProductImages = product.ProductImages,
                Quantity = 1,
                Stock = product.Stock,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(AddProductToCartViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products.FindAsync(model.Id);
            if (product is null)
            {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user is null)
            {
                return NotFound();
            }

            TemporalSale temporalSale = new()
            {
                Product = product,
                Quantity = model.Quantity,
                Remarks = model.Remarks,
                User = user
            };

            _ = _context.TemporalSales.Add(temporalSale);
            _ = await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Add(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            Product product = await _context.Products.FindAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user is null)
            {
                return NotFound();
            }

            TemporalSale temporalSale = new()
            {
                Product = product,
                Quantity = 1,
                User = user,
            };

            _ = _context.TemporalSales.Add(temporalSale);
            _ = await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));


        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ShowCart()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            List<TemporalSale> temporaleSales = await _context.TemporalSales
                                      .Include(ts => ts.Product)
                                      .ThenInclude(p => p.ProductImages)
                                      .Where(ts => ts.User.Id == user.Id)
                                      .ToListAsync();

            ShowCartVIewModel model = new()
            {
                User = user,
                TemporalSales = temporaleSales,
            };

            return View(model);
        }

        public async Task<IActionResult> DecreaseQuantity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TemporalSale temporaleSale = await _context.TemporalSales.FindAsync(id);
            if (temporaleSale is null)
            {
                return NotFound();
            }

            if (temporaleSale.Quantity > 1)
            {
                temporaleSale.Quantity--;
                _ = _context.TemporalSales.Update(temporaleSale);
                _ = await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ShowCart));

        }

        public async Task<IActionResult> IncreaseQuantity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TemporalSale temporaleSale = await _context.TemporalSales.FindAsync(id);
            if (temporaleSale is null)
            {
                return NotFound();
            }

            temporaleSale.Quantity++;
            _ = _context.TemporalSales.Update(temporaleSale);
            _ = await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ShowCart));

        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            TemporalSale temporaleSale = await _context.TemporalSales.FindAsync(id);
            if (temporaleSale is null)
            {
                return NotFound();
            }

            _ = _context.TemporalSales.Remove(temporaleSale);
            _ = await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ShowCart));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            TemporalSale temporaleSale = await _context.TemporalSales.FindAsync(id);
            if (temporaleSale is null)
            {
                 NotFound();
            }

            EditTemporalSaleViewModel model = new()
            {
                Id = temporaleSale.Id,
                Remarks = temporaleSale.Remarks,
                Quantity = temporaleSale.Quantity,
            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id,EditTemporalSaleViewModel model)
        {

            if (id  !=  model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var temporalSale = await _context.TemporalSales.FindAsync(id);
                    temporalSale.Quantity = model.Quantity;
                    temporalSale.Remarks = model.Remarks;
                    _context.Update(temporalSale);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(ShowCart));
                }
                catch (Exception ex)
                {

                    ModelState.AddModelError(String.Empty, ex.Message);
                    return View(model);
                }


            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }
    }
}