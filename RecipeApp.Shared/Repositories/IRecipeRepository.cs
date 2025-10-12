using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RecipeApp.Repositories
{
    public interface IRecipeRepository
    {
        Task InitializeAsync();

        // ---------------------------
        // Recipes
        // ---------------------------
        ObservableCollection<Recipe> Recipes { get; }
        ObservableCollection<Recipe> Favorites { get; }

        Task AddRecipeAsync(Recipe recipe);
        Task UpdateRecipeAsync(Recipe recipe);
        Task<Recipe?> GetRecipeByIdAsync(Guid id);
        Task DeleteRecipeAsync(Guid id);

        Task<bool> AddToFavoritesAsync(Recipe recipe);
        Task<bool> RemoveFromFavoritesAsync(Recipe recipe);

        // ---------------------------
        // Categories
        // ---------------------------
        ObservableCollection<Category> Categories { get; }

        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task DeleteCategoryAsync(Guid id);
    }
}