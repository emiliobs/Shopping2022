using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly DataContext _context;

        public OrdersController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(
                         await _context.Sales
                         .Include(s => s.User)
                         .Include(s => s.SaleDetails)
                         .ThenInclude(sd => sd.Product)
                         .ToListAsync()
                       );
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sales = await _context.Sales
                        .Include(s => s.User)
                        .Include(s => s.SaleDetails)
                        .ThenInclude(sd => sd.Product)
                        .ThenInclude(p => p.ProductImages)
                        .FirstOrDefaultAsync(s => s.Id == id);

            return sales is null ? NotFound() : View(sales);
        }
    }
}
