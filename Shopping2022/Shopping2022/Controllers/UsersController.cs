using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Enums;
using Shopping2022.Helpers;
using Shopping2022.Models;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;

        public UsersController(DataContext context, IUserHelper userHelper, ICombosHelper combosHelper, IBlobHelper blobHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
        }



        public async Task<IActionResult> Index()
        {
            return View(await _context.Users
                         .Include(u => u.City)
                         .ThenInclude(c => c.State)
                         .ThenInclude(s => s.Country)
                         .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            AddUserViewModel addUserViewModel = new AddUserViewModel
            {
                Id = Guid.Empty.ToString(),
                Countries = await _combosHelper.GetComboCountriesAsync(),
                States = await _combosHelper.GetComboStatesByIdAsync(0),
                Cities = await _combosHelper.GetComboCitiesByIdyAsync(0),
                UserType = UserType.Admin,
            };

            return View(addUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel addUserViewModel)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;

                if (addUserViewModel.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(addUserViewModel.ImageFile, "users");
                }

                addUserViewModel.ImageId = imageId;
                Data.Entities.User? user = await _userHelper.AddUserAsync(addUserViewModel);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo Usado.");
                    addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
                    addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.StateId);
                    addUserViewModel.Cities = await _combosHelper.GetComboCitiesByIdyAsync(addUserViewModel.CityId);

                    return View(addUserViewModel);
                }

                return RedirectToAction("Index", "Users");

            }

            addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
            addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.StateId);
            addUserViewModel.Cities = await _combosHelper.GetComboCitiesByIdyAsync(addUserViewModel.CityId);

            return View(addUserViewModel);
        }

        public JsonResult GetStates(int countryId)
        {
            Country country = _context.Countries
                          .Include(c => c.States)
                          .FirstOrDefault(c => c.Id == countryId);
            if (country == null)
            {
                return null;
            }

            return Json(country.States.OrderBy(s => s.Name));

        }

        public JsonResult GetCities(int stateId)
        {
            State state = _context.States.Include(s => s.Cities).FirstOrDefault(s => s.Id == stateId);
            if (state == null)
            {
                return null;
            }

            return Json(state.Cities.OrderBy(s => s.Name));

        }
    }
}
