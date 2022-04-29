using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Common;
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
        private readonly IMailHelper _mailHelper;

        public UsersController(DataContext context, IUserHelper userHelper, ICombosHelper combosHelper, IBlobHelper blobHelper, IMailHelper mailHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
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
            AddUserViewModel addUserViewModel = new()
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

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(
                                                          $"{addUserViewModel.FirstName} {addUserViewModel.LastName}",
                                                          addUserViewModel.Username,
                                                          "Shopping - Confirmación de Email",
                                                          $"<h1>Shopping - Confirmación de Email</h1>" +
                                                          $"Para habilitar el Administrador por favor hacer click en el siguiente link:, " +
                                                          $"<hr/><br><p><a Class='color' href = \"{tokenLink}\" style='color:blue'>Confirmar Email</a></p>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "Las instrucciones para habilitar el Administrador han sido enviadas al Correo.";

                    return View(addUserViewModel);
                }

                ModelState.AddModelError(string.Empty, response.Message);

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
            return country == null ? null : Json(country.States.OrderBy(s => s.Name));
        }

        public JsonResult GetCities(int stateId)
        {
            State state = _context.States.Include(s => s.Cities).FirstOrDefault(s => s.Id == stateId);
            return state == null ? null : Json(state.Cities.OrderBy(s => s.Name));
        }
    }
}
