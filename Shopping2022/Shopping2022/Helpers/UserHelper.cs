using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Models;

namespace Shopping2022.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public UserHelper(DataContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager,
                          SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);    
        }
        public async Task<User> AddUserAsync(AddUserViewModel addUserViewModel)
        {
            var user = new User 
            {
               Address = addUserViewModel.Address,
               Document = addUserViewModel.Document,
               Email = addUserViewModel.Username,
               FirstName = addUserViewModel.FirstName,
               LastName = addUserViewModel.LastName,
               ImageId = addUserViewModel.ImageId,
               PhoneNumber = addUserViewModel.PhoneNumber,
               City = await _context.Cities.FindAsync(addUserViewModel.Id),
               UserName = addUserViewModel.Username,
               UserType = addUserViewModel.UserType
               
            };

            var result = await _userManager.CreateAsync(user, addUserViewModel.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            var newUser = await GetUserAsync(addUserViewModel.Username);
            await AddUserToRoleAsyn(newUser, addUserViewModel.UserType.ToString());

            return newUser;
        }

        public async Task AddUserToRoleAsyn(User user, string roleName)
        {
             await _userManager.AddToRoleAsync(user, roleName);
        }

        public async  Task CheckRoleAsync(string roleName)
        {
           var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole
                {
                    Name = roleName,
                });
            }
        }

        public async Task<User> GetUserAsync(string email)
        {
            return await _context.Users
                        .Include(u => u.City)
                        .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel loginViewModel)
        {
            return await _signInManager.PasswordSignInAsync(loginViewModel.Username,loginViewModel.Password, loginViewModel.RememberMe,false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
