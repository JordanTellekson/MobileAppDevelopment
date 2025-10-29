using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipeApp.Api.Services;
using RecipeApp.Repositories;
using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace RecipeApp.Shared.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public ObservableCollection<User> Users => _userRepo.Users;

        public UserService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task InitializeAsync() => await _userRepo.InitializeAsync();

        public async Task<User?> GetUserByIdAsync(Guid id) =>
            await _userRepo.GetUserByIdAsync(id);

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _userRepo.GetUserByUsernameAsync(username);

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"] ?? "SuperSecretKey12345");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user;
        }

        public async Task<User> RegisterAsync(string username, string password, string role = "User")
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            await _userRepo.AddUserAsync(user);
            if (!Users.Contains(user))
                Users.Add(user);

            return user;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepo.UpdateUserAsync(user);
            var existing = Users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                int index = Users.IndexOf(existing);
                Users[index] = user;
            }
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepo.DeleteUserAsync(id);
            var existing = Users.FirstOrDefault(u => u.Id == id);
            if (existing != null)
                Users.Remove(existing);
        }
    }
}