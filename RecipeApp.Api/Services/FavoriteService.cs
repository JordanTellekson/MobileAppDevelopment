using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecipeApp.Api.Data;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Api.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(AppDbContext context, ILogger<FavoriteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddFavoriteAsync(Guid userId, Guid recipeId)
        {
            // Check if already exists
            bool exists = await _context.UserFavorites
                .AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (exists)
                return false;

            var favorite = new UserFavorite
            {
                UserId = userId,
                RecipeId = recipeId
            };

            _context.UserFavorites.Add(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} added recipe {RecipeId} to favorites", userId, recipeId);
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid recipeId)
        {
            var favorite = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

            if (favorite == null)
                return false;

            _context.UserFavorites.Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} removed recipe {RecipeId} from favorites", userId, recipeId);
            return true;
        }

        public async Task<IEnumerable<Recipe>> GetUserFavoritesAsync(Guid userId)
        {
            return await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Recipe)
                .Select(f => f.Recipe!)
                .ToListAsync();
        }
    }
}