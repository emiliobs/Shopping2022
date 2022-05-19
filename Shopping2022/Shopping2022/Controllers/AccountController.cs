using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Common;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Enums;
using Shopping2022.Helpers;
using Shopping2022.Models;
using Vereyon.Web;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Shopping2022.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IFlashMessage _flashMessage;

        public AccountController(IUserHelper userHelper, DataContext context, ICombosHelper combosHelper,
                                IBlobHelper blobHelper, IMailHelper mailHelper, IFlashMessage flashMessage)
        {
            _userHelper = userHelper;
            _context = context;
            _combosHelper = combosHelper;
            _blobHelper = blobHelper;
            _mailHelper = mailHelper;
            _flashMessage = flashMessage;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                SignInResult result = await _userHelper.LoginAsync(loginViewModel);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    _flashMessage.Danger("Ha superado el máximo número de intentos, su cuenta está bloqeada, intente de nuevo en 5 minutos.");
                }
                else if (result.IsNotAllowed)
                {
                    _flashMessage.Danger("El usuario no ha sido habilitado, debes de seguir las instrucciones del correo enviado para poder habilitarte en el Sistema.");
                }
                else
                {

                    _flashMessage.Danger("Email o Contraseña incorrectos");
                }

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
                User user = await _userHelper.AddUserAsync(addUserViewModel);
                if (user == null)
                {
                    _flashMessage.Danger("Este correo ya está siendo usado.");

                    addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
                    addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.CountryId);
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
                                                          $"Para habilitar el usuario por favor hacer click en el siguiente link:, " +
                                                          $"<hr/><br><p><a Class=\"color btn btn-primary\" href = \"{tokenLink}\" style='color:blue'>Confirmar Email</a></p>");

                if (response.IsSuccess)
                {
                    _flashMessage.Danger("Las instrucciones para habilitar el usuario han sido enviadas al Correo.");


                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, response.Message);

            }

            addUserViewModel.Countries = await _combosHelper.GetComboCountriesAsync();
            addUserViewModel.States = await _combosHelper.GetComboStatesByIdAsync(addUserViewModel.CountryId);
            addUserViewModel.Cities = await _combosHelper.GetComboCitiesByIdyAsync(addUserViewModel.CityId);

            return View(addUserViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            User user = await _userHelper.GetUserAsync(new Guid(userId));
            if (user is null)
            {
                return NotFound();
            }

            IdentityResult result = await _userHelper.ConfirmEmailAsync(user, token);
            return !result.Succeeded ? NotFound() : View(result);
        }

        public async Task<IActionResult> ChangeUser()
        {
            User user = await _userHelper.GetUserAsync(User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            EditUserViewModel model = new()
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
                StateId = user.City.State.Id,
                States = await _combosHelper.GetComboStatesByIdAsync(user.City.State.Country.Id),
                Id = user.Id,
                Document = user.Document
            };

            return View(model);
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

                _ = await _userHelper.UpdateUserAsync(user);



                return RedirectToAction("Index", "Home");
            }

            model.Countries = await _combosHelper.GetComboCountriesAsync();
            model.States = await _combosHelper.GetComboStatesByIdAsync(model.CountryId);
            model.Cities = await _combosHelper.GetComboCitiesByIdyAsync(model.StateId);

            return View(model);

        }


        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.OldPassword == model.NewPassword)
                {

                    _flashMessage.Danger("Debes ingresar una contraseña diferente.");

                    return View(model);
                }

                User user = await _userHelper.GetUserAsync(User.Identity.Name);

                if (user != null)
                {
                    IdentityResult result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("ChangeUser");
                    }
                    else
                    {
                        _flashMessage.Danger(result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    _flashMessage.Danger("Usuario no Encontrado.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user is null)
                {
                    _flashMessage.Danger("El Email no corresponde a ningun usuario registrado.");

                    return View(model);
                }

                string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                string tokenLink = Url.Action("ResetPassword", "Account", new
                {
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendMail(
                                                          $"{user.FullName}",
                                                          model.Email,
                                                          "Shopping - Recuperación de Contraseña",
                                                          $"<h1>Shopping - Recuperación de Contraseña</h1>" +
                                                          $"Para recuperar la contraseña favor hacer click en el siguiente link:, " +
                                                          $"<hr/><br><p><a Class=\"color btn btn-primary\" href = \"{tokenLink}\" style='color:blue'>Reset Password</a></p>");

                if (response.IsSuccess)
                {
                    _flashMessage.Confirmation("Las instrucciones para recuperar la contraseña han sido enviadas a su Correo.");
                    return RedirectToAction(nameof(Login));
                }

                _flashMessage.Danger(response.Message);

                //ModelState.AddModelError(string.Empty, response.Message);

            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.UserName);
                if (user is not null)
                {
                    IdentityResult result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        _flashMessage.Confirmation("Contraseña cambiada con éxito");

                        return RedirectToAction(nameof(Login));

                    }

                    _flashMessage.Danger("Error cambiando la Contraseña.");

                    return View(model);

                }

                return View(model);
            }

            _flashMessage.Danger("Usuario no encontrado.");

            return View(model);
        }

        public JsonResult GetStates(int countryId)
        {
            Country? country = _context.Countries
                .Include(c => c.States).FirstOrDefault(c => c.Id == countryId);

            return country == null ? null : Json(country.States.OrderBy(d => d.Name));
        }

        public JsonResult GetCities(int stateId)
        {
            State? state = _context.States
                .Include(s => s.Cities)
                .FirstOrDefault(s => s.Id == stateId);
            return state == null ? null : Json(state.Cities.OrderBy(c => c.Name));
        }



    }
}
