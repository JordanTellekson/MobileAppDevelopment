using RecipeApp.Repositories;
using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ObservableCollection<Recipe> Recipes => _recipeRepo.Recipes;
        public ObservableCollection<Recipe> Favorites => _recipeRepo.Favorites;
        public ObservableCollection<Category> Categories => _categoryRepo.Categories;

        public RecipeService(IRecipeRepository recipeRepo, ICategoryRepository categoryRepo)
        {
            _recipeRepo = recipeRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task InitializeAsync()
        {
            // Initialize repositories first
            await _recipeRepo.InitializeAsync();
            await _categoryRepo.InitializeAsync();

            // Dynamically populate categories from existing recipes
            var existingCategories = Recipes
                .Select(r => r.CategoryName)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            foreach (var categoryName in existingCategories)
            {
                // Only add if it doesn't already exist
                if (!Categories.Any(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    await AddCategoryAsync(new Category { Id = Guid.NewGuid(), Name = categoryName });
                }
            }
        }

        // ---------------------------
        // Recipes
        // ---------------------------
        public Task AddRecipeAsync(Recipe recipe)
        {
            // Ensure category exists in Categories list
            if (!string.IsNullOrWhiteSpace(recipe.CategoryName) &&
                !Categories.Any(c => string.Equals(c.Name, recipe.CategoryName, StringComparison.OrdinalIgnoreCase)))
            {
                _categoryRepo.AddCategoryAsync(new Category { Id = Guid.NewGuid(), Name = recipe.CategoryName });
            }

            return _recipeRepo.AddRecipeAsync(recipe);
        }

        public Task UpdateRecipeAsync(Recipe recipe) => _recipeRepo.UpdateRecipeAsync(recipe);
        public Task DeleteRecipeAsync(Guid id) => _recipeRepo.DeleteRecipeAsync(id);
        public Task<Recipe?> GetRecipeByIdAsync(Guid id) => _recipeRepo.GetRecipeByIdAsync(id);

        public Task<bool> AddToFavoritesAsync(Recipe recipe) => _recipeRepo.AddToFavoritesAsync(recipe);
        public Task<bool> RemoveFromFavoritesAsync(Recipe recipe) => _recipeRepo.RemoveFromFavoritesAsync(recipe);

        // ---------------------------
        // Categories
        // ---------------------------
        public Task AddCategoryAsync(Category category) => _categoryRepo.AddCategoryAsync(category);
        public Task UpdateCategoryAsync(Category category) => _categoryRepo.UpdateCategoryAsync(category);
        public Task DeleteCategoryAsync(Guid id) => _categoryRepo.DeleteCategoryAsync(id);
        public Task<Category?> GetCategoryByIdAsync(Guid id) => _categoryRepo.GetCategoryByIdAsync(id);
    }
}