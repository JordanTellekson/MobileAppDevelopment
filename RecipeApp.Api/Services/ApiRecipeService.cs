using RecipeApp.Repositories;
using RecipeApp.Shared.Services;
using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Api.Services
{
    public class ApiRecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepo;

        public ObservableCollection<Recipe> Recipes => _recipeRepo.Recipes;
        public ObservableCollection<Recipe> Favorites => _recipeRepo.Favorites;
        public ObservableCollection<Category> Categories => _recipeRepo.Categories;

        public ApiRecipeService(IRecipeRepository recipeRepo)
        {
            _recipeRepo = recipeRepo;
        }

        public async Task InitializeAsync()
        {
            await _recipeRepo.InitializeAsync();

            // Ensure all recipes link correctly to their category objects
            foreach (var recipe in Recipes)
            {
                if (recipe.CategoryId.HasValue && recipe.Category == null)
                {
                    recipe.Category = Categories.FirstOrDefault(c => c.Id == recipe.CategoryId.Value);
                }
            }

            // Add any missing categories dynamically (e.g., from manually created recipes)
            var missingCategories = Recipes
                .Where(r => r.Category != null && !Categories.Any(c => c.Id == r.Category.Id))
                .Select(r => r.Category)
                .Distinct()
                .ToList();

            foreach (var category in missingCategories)
            {
                await AddCategoryAsync(category);
            }
        }

        // ---------------------------
        // Recipes
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe.Category != null)
            {
                // Ensure category exists before linking
                var existingCategory = Categories.FirstOrDefault(c => c.Id == recipe.Category.Id);
                if (existingCategory == null)
                {
                    await AddCategoryAsync(recipe.Category);
                }
                else
                {
                    recipe.Category = existingCategory;
                    recipe.CategoryId = existingCategory.Id;
                }
            }

            await _recipeRepo.AddRecipeAsync(recipe);
        }

        public Task UpdateRecipeAsync(Recipe recipe) => _recipeRepo.UpdateRecipeAsync(recipe);
        public Task DeleteRecipeAsync(Guid id) => _recipeRepo.DeleteRecipeAsync(id);
        public Task<Recipe?> GetRecipeByIdAsync(Guid id) => _recipeRepo.GetRecipeByIdAsync(id);

        public Task<bool> AddToFavoritesAsync(Recipe recipe) => _recipeRepo.AddToFavoritesAsync(recipe);
        public Task<bool> RemoveFromFavoritesAsync(Recipe recipe) => _recipeRepo.RemoveFromFavoritesAsync(recipe);

        // ---------------------------
        // Categories
        // ---------------------------
        public async Task AddCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            if (!Categories.Any(c => c.Id == category.Id))
            {
                Categories.Add(category);
                await _recipeRepo.AddCategoryAsync(category);
            }
        }

        public Task UpdateCategoryAsync(Category category) => _recipeRepo.UpdateCategoryAsync(category);
        public Task DeleteCategoryAsync(Guid id) => _recipeRepo.DeleteCategoryAsync(id);
        public Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            var category = Categories.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(category);
        }
    }
}