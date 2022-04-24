using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Models;
using System.Diagnostics;

namespace Shopping2022.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;

        public HomeController(ILogger<HomeController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(p => p.ProductImages)
                                                  .Include(p => p.ProductCategories)
                                                  .OrderBy(p => p.Description).ToListAsync();

            List<ProductHomeViewModel> productHome = new List<ProductHomeViewModel>() { new ProductHomeViewModel()};
            
            int i = 1;
            foreach (Product product in products)
            {
                if (i == 1)
                {
                    productHome.LastOrDefault().Product1 = product;
                }

                if (i == 2)
                {
                    productHome.LastOrDefault().Product2 = product;
                }

                if (i == 3)
                {
                    productHome.LastOrDefault().Product3 = product;
                }

                if (i == 4)
                {
                    productHome.LastOrDefault().Product4 = product;
                    productHome.Add(new ProductHomeViewModel());

                    i = 0;
                }

                i++;
            }


            return View(productHome);
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