using Microsoft.AspNetCore.Identity;
using Shopping2022.Data.Entities;
using Shopping2022.Models;

namespace Shopping2022.Helpers
{
    public interface IUserHelper
    {
        Task<User> GetUserAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);
        Task<User> AddUserAsync(AddUserViewModel addUserViewModel);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsyn(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);
        
        Task<SignInResult> LoginAsync(LoginViewModel loginViewModel);

        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

        Task<IdentityResult> UpdateUserAsync(User user);

        Task<User> GetUserAsync(Guid userId);

        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        Task<IdentityResult> ConfirmEmailAsync(User user, string token);

        Task LogoutAsync();
    } 
}
