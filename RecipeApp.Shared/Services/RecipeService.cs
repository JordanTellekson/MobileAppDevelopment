using RecipeApp.Repositories;
using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Shared.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepo;

        public ObservableCollection<Recipe> Recipes => _recipeRepo.Recipes;
        public ObservableCollection<Recipe> Favorites => _recipeRepo.Favorites;
        public ObservableCollection<Category> Categories => _recipeRepo.Categories;

        public RecipeService(IRecipeRepository recipeRepo)
        {
            _recipeRepo = recipeRepo;
        }

        /// <summary>
        /// Initialize the service by fetching all recipes and categories from the API.
        /// Links recipes to their categories and populates Favorites.
        /// </summary>
        public async Task InitializeAsync()
        {
            await _recipeRepo.InitializeAsync();

            // Link each recipe to its Category object
            foreach (var recipe in Recipes)
            {
                if (recipe.CategoryId.HasValue && recipe.Category == null)
                {
                    recipe.Category = Categories.FirstOrDefault(c => c.Id == recipe.CategoryId.Value);
                }
            }

            // Ensure Favorites collection is accurate
            Favorites.Clear();
            foreach (var fav in Recipes.Where(r => r.IsFavorite))
                Favorites.Add(fav);
        }

        // ---------------------------
        // Recipes CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe.Category != null)
                recipe.CategoryId = recipe.Category.Id;

            await _recipeRepo.AddRecipeAsync(recipe);

            // Keep collections in sync
            if (!Recipes.Contains(recipe))
                Recipes.Add(recipe);
            if (recipe.IsFavorite && !Favorites.Contains(recipe))
                Favorites.Add(recipe);
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            await _recipeRepo.UpdateRecipeAsync(recipe);

            var existing = Recipes.FirstOrDefault(r => r.Id == recipe.Id);
            if (existing != null)
            {
                int index = Recipes.IndexOf(existing);
                Recipes[index] = recipe;

                if (recipe.IsFavorite)
                {
                    if (!Favorites.Contains(recipe))
                        Favorites.Add(recipe);
                }
                else
                {
                    Favorites.Remove(recipe);
                }
            }
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            await _recipeRepo.DeleteRecipeAsync(id);

            var existing = Recipes.FirstOrDefault(r => r.Id == id);
            if (existing != null)
            {
                Recipes.Remove(existing);
                Favorites.Remove(existing);
            }
        }

        public Task<Recipe?> GetRecipeByIdAsync(Guid id) => _recipeRepo.GetRecipeByIdAsync(id);

        public async Task<bool> AddToFavoritesAsync(Recipe recipe)
        {
            var success = await _recipeRepo.AddToFavoritesAsync(recipe);
            if (success && !Favorites.Contains(recipe))
            {
                recipe.IsFavorite = true;
                Favorites.Add(recipe);
            }
            return success;
        }

        public async Task<bool> RemoveFromFavoritesAsync(Recipe recipe)
        {
            var success = await _recipeRepo.RemoveFromFavoritesAsync(recipe);
            if (success)
            {
                recipe.IsFavorite = false;
                Favorites.Remove(recipe);
            }
            return success;
        }

        // ---------------------------
        // Categories CRUD
        // ---------------------------
        public async Task AddCategoryAsync(Category category)
        {
            await _recipeRepo.AddCategoryAsync(category);
            if (!Categories.Contains(category))
                Categories.Add(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            await _recipeRepo.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            await _recipeRepo.DeleteCategoryAsync(id);
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return await _recipeRepo.GetCategoryByIdAsync(id);
        }

        public async Task<IEnumerable<Recipe>> InitializeAndGetAllAsync()
        {
            await InitializeAsync();
            return Recipes;
        }
    }
}