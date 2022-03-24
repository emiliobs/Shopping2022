using Microsoft.AspNetCore.Identity;
using Shopping2022.Data.Entities;

namespace Shopping2022.Helpers
{
    public interface IUserHelper
    {
        Task<User> GetUserAsync(string email);

        Task<IdentityResult> AddUserAsync(User user, string password);

        Task CheckRoleAsync(string roleName);

        Task AddUserToRoleAsyn(User user, string roleName);

        Task<bool> IsUserInRoleAsync(User user, string roleName);
    } 
}
