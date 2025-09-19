using RecipeApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public interface IRecipeRepository
    {
        ObservableCollection<Recipe> Recipes { get; }
        ObservableCollection<Recipe> Favorites { get; }

        Task AddRecipeAsync(Recipe recipe);
        Task UpdateRecipeAsync(Recipe recipe);
        Task<Recipe?> GetRecipeByIdAsync(Guid id);
        Task DeleteRecipeAsync(Guid id);

        bool AddToFavorites(Recipe recipe);
        bool RemoveFromFavorites(Recipe recipe);
    }
}