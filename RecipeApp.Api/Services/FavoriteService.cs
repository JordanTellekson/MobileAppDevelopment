using Microsoft.Extensions.Logging;
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
        private readonly IRecipeService _recipeService;
        private readonly ILogger<FavoriteService> _logger;

        // In-memory simulation for now (we’ll make it persistent later)
        private static readonly Dictionary<Guid, HashSet<Guid>> _userFavorites = new();

        public FavoriteService(IRecipeService recipeService, ILogger<FavoriteService> logger)
        {
            _recipeService = recipeService;
            _logger = logger;
        }

        public async Task<bool> AddFavoriteAsync(Guid userId, Guid recipeId)
        {
            if (!_userFavorites.ContainsKey(userId))
                _userFavorites[userId] = new HashSet<Guid>();

            var userFavs = _userFavorites[userId];
            if (userFavs.Contains(recipeId))
                return false; // already a favorite

            userFavs.Add(recipeId);
            _logger.LogInformation("User {UserId} added recipe {RecipeId} to favorites", userId, recipeId);
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid recipeId)
        {
            if (!_userFavorites.ContainsKey(userId))
                return false;

            var userFavs = _userFavorites[userId];
            bool removed = userFavs.Remove(recipeId);
            if (removed)
                _logger.LogInformation("User {UserId} removed recipe {RecipeId} from favorites", userId, recipeId);

            return removed;
        }

        public async Task<IEnumerable<Recipe>> GetUserFavoritesAsync(Guid userId)
        {
            if (!_userFavorites.ContainsKey(userId))
                return Enumerable.Empty<Recipe>();

            var favoriteIds = _userFavorites[userId];
            var recipes = await _recipeService.InitializeAndGetAllAsync(); // we’ll add this helper method next

            return recipes.Where(r => favoriteIds.Contains(r.Id));
        }
    }
}