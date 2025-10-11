using RecipeApp.Repositories;
using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
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
            await _categoryRepo.InitializeAsync();
            await _recipeRepo.InitializeAsync();
        }

        // ---------------------------
        // Recipes
        // ---------------------------
        public Task AddRecipeAsync(Recipe recipe) => _recipeRepo.AddRecipeAsync(recipe);
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