using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RecipeApp.Shared.Services
{
    public interface IRecipeService
    {
        ObservableCollection<Recipe> Recipes { get; }
        ObservableCollection<Recipe> Favorites { get; }
        ObservableCollection<Category> Categories { get; }
        Task InitializeAsync();
        Task AddRecipeAsync(Recipe recipe);
        Task UpdateRecipeAsync(Recipe recipe);
        Task DeleteRecipeAsync(Guid id);
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(Guid id);
        Task<Recipe?> GetRecipeByIdAsync(Guid id);
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<bool> AddToFavoritesAsync(Recipe recipe);
        Task<bool> RemoveFromFavoritesAsync(Recipe recipe);
    }
}