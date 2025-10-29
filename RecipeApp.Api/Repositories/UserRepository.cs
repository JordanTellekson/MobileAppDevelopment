using RecipeApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RecipeApp.Api.Data;

namespace RecipeApp.Repositories
{
    public class ApiUserRepository : IUserRepository
    {
        private readonly ILogger<ApiUserRepository> _logger;
        private readonly AppDbContext _dbContext;

        public ObservableCollection<User> Users { get; } = new();

        public ApiUserRepository(ILogger<ApiUserRepository> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        // ---------------------------
        // Initialization
        // ---------------------------
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing SQL UserRepository...");

            var users = await _dbContext.Users.ToListAsync();
            Users.Clear();
            foreach (var u in users)
                Users.Add(u);

            _logger.LogInformation("Loaded {Count} users from SQL.", Users.Count);
        }

        // ---------------------------
        // CRUD
        // ---------------------------
        public async Task AddUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (user.Id == Guid.Empty) user.Id = Guid.NewGuid();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            Users.Add(user);
            _logger.LogInformation("✅ Added user: {Username}", user.Username);
        }

        public async Task UpdateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var existing = await _dbContext.Users.FindAsync(user.Id);
            if (existing != null)
            {
                _dbContext.Entry(existing).CurrentValues.SetValues(user);
                await _dbContext.SaveChangesAsync();

                var obs = Users.First(u => u.Id == user.Id);
                obs.Username = user.Username;
                obs.PasswordHash = user.PasswordHash;
                obs.Role = user.Role;

                _logger.LogInformation("Updated user: {Username}", user.Username);
            }
            else
            {
                _logger.LogWarning("Update failed: User with Id {Id} not found", user.Id);
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            _logger.LogDebug("GetUserByIdAsync({Id}) -> Found: {Found}", id, user != null);
            return user;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            _logger.LogDebug("GetUserByUsernameAsync({Username}) -> Found: {Found}", username, user != null);
            return user;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();

                Users.Remove(user);
                _logger.LogInformation("Deleted user: {Username}", user.Username);
            }
            else
            {
                _logger.LogWarning("Delete failed: User with Id {Id} not found", id);
            }
        }
    }
}