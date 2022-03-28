using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
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
                Guid imageId = Guid.Empty;

                if (addUserViewModel.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(addUserViewModel.ImageFile, "users");
                }

                addUserViewModel.ImageId = imageId;
                User? user = await _userHelper.AddUserAsync(addUserViewModel);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Este correo ya está siendo usado.");

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

                Microsoft.AspNetCore.Identity.SignInResult? result = await _userHelper.LoginAsync(loginViewModel);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
            addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.CountryId);
            addUserViewModel.Cities = await _combosHelper.GetComboCitiesByIdyAsync(addUserViewModel.CityId);

            return View(addUserViewModel);
        }


        [HttpGet]
        public async Task<IActionResult> ChangeUser()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            EditUserViewModel editUserViewModel = new EditUserViewModel
            {
                Address = user.Address,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageId = user.ImageId,
                Cities = await _combosHelper.GetComboCitiesByIdyAsync(user.City.State.Id),
                CityId = user.City.Id,
                Countries = await _combosHelper.GetComboCountriesAsync(),
                CountryId = user.City.State.Country.Id,
                States = await _combosHelper.GetComboStatesByIdAsync(user.City.State.Country.Id),
                StateId = user.City.State.Id,
                Id = user.Id,
                Document = user.Document
            };




            return View(editUserViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = model.ImageId;

                if (model.ImageFile != null)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                }

                User user = await _userHelper.GetUserAsync(User.Identity.Name);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.ImageId = imageId;
                user.City = await _context.Cities.FindAsync(model.CityId);
                user.Document = model.Document;

                await _userHelper.UpdateUserAsync(user);



                return RedirectToAction("Index", "Home");
            }

            model.Countries = await _combosHelper.GetComboCountriesAsync();
            model.States = await _combosHelper.GetComboStatesByIdAsync(model.CountryId);
            model.Cities = await _combosHelper.GetComboCitiesByIdyAsync(model.StateId);

            return View(model);

        }

        public JsonResult GetStates(int countryId)
        {
            Country? country = _context.Countries
                .Include(c => c.States).FirstOrDefault(c => c.Id == countryId);

            if (country == null)
            {
                return null;
            }

            return Json(country.States.OrderBy(d => d.Name));
        }

        public JsonResult GetCities(int stateId)
        {
            State? state = _context.States
                .Include(s => s.Cities)
                .FirstOrDefault(s => s.Id == stateId);
            if (state == null)
            {
                return null;
            }

            return Json(state.Cities.OrderBy(c => c.Name));
        }



    }
}
