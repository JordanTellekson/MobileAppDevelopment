using RecipeApp.Shared.Models;
using System;
using System.Threading.Tasks;

namespace RecipeApp.Api.Services
{
    public interface IUserService
    {
        Task InitializeAsync(); // load users from repo
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(string username, string password, string role = "User");
        Task<bool> ValidateUserAsync(string username, string password);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);
        Task<User?> GetUserByIdAsync(Guid id);
    }
}