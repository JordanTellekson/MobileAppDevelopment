using RecipeApp.Repositories;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Api.Services
{
    public class ApiRecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ObservableCollection<Recipe> Recipes => _recipeRepo.Recipes;
        public ObservableCollection<Recipe> Favorites => _recipeRepo.Favorites;
        public ObservableCollection<Category> Categories => _categoryRepo.Categories;

        public ApiRecipeService(IRecipeRepository recipeRepo, ICategoryRepository categoryRepo)
        {
            _recipeRepo = recipeRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task InitializeAsync()
        {
            // Initialize repositories first
            await _categoryRepo.InitializeAsync();
            await _recipeRepo.InitializeAsync();

            // Ensure recipes have proper Category navigation reference
            foreach (var recipe in Recipes)
            {
                if (recipe.CategoryId.HasValue && recipe.Category == null)
                {
                    recipe.Category = Categories.FirstOrDefault(c => c.Id == recipe.CategoryId.Value);
                }
            }
        }

        // ---------------------------
        // Recipes
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            // Ensure recipe has a category reference
            if (recipe.Category != null)
            {
                recipe.CategoryId = recipe.Category.Id;

                // Add category to repo if missing
                if (!Categories.Any(c => c.Id == recipe.Category.Id))
                    await _categoryRepo.AddCategoryAsync(recipe.Category);
            }

            await _recipeRepo.AddRecipeAsync(recipe);
        }

        public Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe.Category != null)
            {
                recipe.CategoryId = recipe.Category.Id;
            }

            return _recipeRepo.UpdateRecipeAsync(recipe);
        }

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