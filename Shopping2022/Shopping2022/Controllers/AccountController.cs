using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Enums;
using Shopping2022.Helpers;
using Shopping2022.Models;

namespace Shopping2022.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;

        public AccountController(IUserHelper userHelper, DataContext context, ICombosHelper combosHelper, IBlobHelper blobHelper)
        {
            _userHelper = userHelper;
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult? result = await _userHelper.LoginAsync(loginViewModel);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email o Contraseña incorrectos");

            }
            return View(loginViewModel);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            AddUserViewModel addUserViewModel = new()
            {
                Id = Guid.Empty.ToString(),
                Countries = await _combosHelper.GetComboCountriesAsync(),
                States = await _combosHelper.GetComboStatesByIdAsync(0),
                Cities = await _combosHelper.GetComboCitiesByIdyAsync(0),
                UserType = UserType.User,


            };

            return View(addUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddUserViewModel addUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var imageId = Guid.Empty;

                if (addUserViewModel.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(addUserViewModel.ImageFile, "users");
                }

                addUserViewModel.ImageId = imageId;
                var user = await _userHelper.AddUserAsync(addUserViewModel);
                if (user == null)
                {
                    ModelState.AddModelError(String.Empty, "Este correo ya está siendo usado.");

                    addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
                    addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.CountryId);
                    addUserViewModel.Cities = await _combosHelper.GetComboCitiesByIdyAsync(addUserViewModel.CityId);

                    return View(addUserViewModel);
                }

                LoginViewModel loginViewModel = new LoginViewModel
                {
                    Password = addUserViewModel.Password,
                    RememberMe = false,
                    Username = addUserViewModel.Username,
                };

                var result = await _userHelper.LoginAsync(loginViewModel);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

            }

            addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
            addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.CountryId);
            addUserViewModel.Cities = await _combosHelper.GetComboCitiesByIdyAsync(addUserViewModel.CityId);

            return View(addUserViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var editUserViewModel = new EditUserViewModel
            {
                Address = user.Address,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageId = user.ImageId,
                Countries = await _combosHelper.GetComboCountriesAsync(),
                CountryId = user.City.State.Country.Id,
                States = await _combosHelper.GetComboStatesByIdAsync(user.City.State.Country.Id),
                StateId = user.City.State.Id,
                Cities = await _combosHelper.GetComboCitiesByIdyAsync(user.City.State.Id),
                CityId = user.City.Id,
                Id = user.Id,
                Document = user.Document,
            };

            return View(editUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(EditUserViewModel editUserViewModel)
        {
            if (ModelState.IsValid)
            {
                var imageId = editUserViewModel.ImageId;

                if (editUserViewModel.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(editUserViewModel.ImageFile, "users");
                }

                var user = await _userHelper.GetUserAsync(User.Identity.Name);

                user.FirstName = editUserViewModel.FirstName;
                user.LastName = editUserViewModel.LastName;
                user.Document = editUserViewModel.Document;
                user.Address = editUserViewModel.Address;
                user.PhoneNumber = editUserViewModel.PhoneNumber;
                user.ImageId = imageId;
                user.City = await _context.Cities.FindAsync(editUserViewModel.CityId);

                await _userHelper.UpdateUserAsync(user);

                return RedirectToAction("Index", "Home");
            }

            editUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
            editUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(editUserViewModel.CountryId);
            editUserViewModel.Countries = await _combosHelper.GetComboCitiesByIdyAsync(editUserViewModel.StateId);

            return View(editUserViewModel);

        }

        public JsonResult GetStates(int countryId)
        {
            var country = _context.Countries
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
            var state = _context.States
                        .Include(s => s.Cities)
                        .FirstOrDefault(s => s.Id == stateId);

            if (state == null)
            {
                return null;
            }

            return Json(state.Cities.OrderBy(s => s.Name));
        }

    }
}
