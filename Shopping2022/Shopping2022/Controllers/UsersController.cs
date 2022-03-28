using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly DataContext _context;

        public UsersController(DataContext context)
        {
            this._context = context;
        }



        public async Task<IActionResult> Index() => View(await _context.Users
                                                               .Include(u => u.City)
                                                               .ThenInclude(c => c.State)
                                                               .ThenInclude(s => s.Country)
                                                               .ToListAsync());
    }
}
