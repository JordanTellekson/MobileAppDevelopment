using RecipeApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeApp.Shared.Services
{
    public interface IFavoriteService
    {
        Task<bool> AddFavoriteAsync(Guid userId, Guid recipeId);
        Task<bool> RemoveFavoriteAsync(Guid userId, Guid recipeId);
        Task<IEnumerable<Recipe>> GetUserFavoritesAsync(Guid userId);
    }
}