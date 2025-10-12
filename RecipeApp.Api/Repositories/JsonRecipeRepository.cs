using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecipeApp.Repositories
{
    public class JsonRecipeRepository : IRecipeRepository
    {
        private readonly ILogger<JsonRecipeRepository> _logger;
        private readonly string _filePath;
        private JsonDataStore _dataStore = new(); // Holds both recipes and categories

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public JsonRecipeRepository(ILogger<JsonRecipeRepository> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _filePath = Path.Combine(env.ContentRootPath, "Data", "Recipes.json");

            if (!File.Exists(_filePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                File.WriteAllText(_filePath, JsonSerializer.Serialize(_dataStore));
            }
        }

        // ---------------------------
        // Initialization
        // ---------------------------
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing JSON RecipeRepository...");

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                _dataStore = JsonSerializer.Deserialize<JsonDataStore>(json) ?? new JsonDataStore();

                // Populate categories
                Categories.Clear();
                foreach (var c in _dataStore.Categories)
                    Categories.Add(c);

                // Populate recipes and link categories
                Recipes.Clear();
                Favorites.Clear();
                foreach (var r in _dataStore.Recipes)
                {
                    if (r.CategoryId.HasValue)
                        r.Category = Categories.FirstOrDefault(c => c.Id == r.CategoryId.Value);

                    Recipes.Add(r);
                    if (r.IsFavorite)
                        Favorites.Add(r);
                }

                _logger.LogInformation("Loaded {Count} recipes and {CatCount} categories.", Recipes.Count, Categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing JSON repository");
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                // Sync CategoryId before saving
                foreach (var r in _dataStore.Recipes)
                    r.CategoryId = r.Category?.Id;

                string json = JsonSerializer.Serialize(_dataStore, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_filePath, json);
                _logger.LogInformation("Saved {Count} recipes and {CatCount} categories to JSON.", Recipes.Count, Categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving JSON repository");
            }
        }

        // ---------------------------
        // CATEGORY CRUD
        // ---------------------------
        public async Task AddCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (category.Id == Guid.Empty) category.Id = Guid.NewGuid();

            _dataStore.Categories.Add(category);
            Categories.Add(category);

            _logger.LogInformation("Added category: {Name}", category.Name);
            await SaveAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            var existing = _dataStore.Categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
                var obs = Categories.First(c => c.Id == category.Id);
                obs.Name = category.Name;

                _logger.LogInformation("Updated category: {Name}", category.Name);
                await SaveAsync();
            }
            else
            {
                _logger.LogWarning("Update failed: Category with Id {Id} not found", category.Id);
            }
        }

        public Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            var category = _dataStore.Categories.FirstOrDefault(c => c.Id == id);
            _logger.LogDebug("GetCategoryByIdAsync({Id}) -> Found: {Found}", id, category != null);
            return Task.FromResult(category);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = _dataStore.Categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _dataStore.Categories.Remove(category);
                Categories.Remove(category);

                // Clear category references in recipes
                foreach (var recipe in _dataStore.Recipes.Where(r => r.CategoryId == id))
                {
                    recipe.Category = null;
                    recipe.CategoryId = null;
                }

                _logger.LogInformation("Deleted category: {Name}", category.Name);
                await SaveAsync();
            }
            else
            {
                _logger.LogWarning("Delete failed: Category with Id {Id} not found", id);
            }
        }

        // ---------------------------
        // RECIPE CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (recipe.Id == Guid.Empty) recipe.Id = Guid.NewGuid();

            _dataStore.Recipes.Add(recipe);
            Recipes.Add(recipe);
            if (recipe.IsFavorite) Favorites.Add(recipe);

            _logger.LogInformation("Added recipe: {Title}", recipe.Title);
            await SaveAsync();
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            var existing = _dataStore.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
            if (existing != null)
            {
                existing.Title = recipe.Title;
                existing.Description = recipe.Description;
                existing.ImageUrl = recipe.ImageUrl;
                existing.CookingTimeMinutes = recipe.CookingTimeMinutes;
                existing.Ingredients = recipe.Ingredients;
                existing.Instructions = recipe.Instructions;
                existing.Author = recipe.Author;
                existing.IsFavorite = recipe.IsFavorite;
                existing.Category = recipe.Category;
                existing.CategoryId = recipe.Category?.Id;

                var obs = Recipes.First(r => r.Id == recipe.Id);
                obs.Title = recipe.Title;
                obs.Description = recipe.Description;
                obs.ImageUrl = recipe.ImageUrl;
                obs.CookingTimeMinutes = recipe.CookingTimeMinutes;
                obs.Ingredients = recipe.Ingredients;
                obs.Instructions = recipe.Instructions;
                obs.Author = recipe.Author;
                obs.IsFavorite = recipe.IsFavorite;
                obs.Category = recipe.Category;
                obs.CategoryId = recipe.Category?.Id;

                if (recipe.IsFavorite && !Favorites.Any(r => r.Id == recipe.Id))
                    Favorites.Add(obs);
                else if (!recipe.IsFavorite)
                    Favorites.Remove(obs);

                _logger.LogInformation("Updated recipe: {Title}", recipe.Title);
                await SaveAsync();
            }
            else
            {
                _logger.LogWarning("Update failed: Recipe with Id {Id} not found", recipe.Id);
            }
        }

        public Task<Recipe?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = _dataStore.Recipes.FirstOrDefault(r => r.Id == id);
            _logger.LogDebug("GetRecipeByIdAsync({Id}) -> Found: {Found}", id, recipe != null);
            return Task.FromResult(recipe);
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            var recipe = _dataStore.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe != null)
            {
                _dataStore.Recipes.Remove(recipe);
                Recipes.Remove(recipe);
                Favorites.Remove(recipe);

                _logger.LogInformation("Deleted recipe: {Title}", recipe.Title);
                await SaveAsync();
            }
            else
            {
                _logger.LogWarning("Delete failed: Recipe with Id {Id} not found", id);
            }
        }

        // ---------------------------
        // FAVORITES
        // ---------------------------
        public async Task<bool> AddToFavoritesAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("AddToFavoritesAsync called with null recipe");
                return false;
            }

            if (Favorites.Any(r => r.Id == recipe.Id))
            {
                _logger.LogInformation("Recipe {Title} is already a favorite", recipe.Title);
                return false;
            }

            recipe.IsFavorite = true;
            Favorites.Add(recipe);

            await SaveAsync();
            _logger.LogInformation("Added recipe to favorites: {Title}", recipe.Title);
            return true;
        }

        public async Task<bool> RemoveFromFavoritesAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("RemoveFromFavoritesAsync called with null recipe");
                return false;
            }

            recipe.IsFavorite = false;
            Favorites.Remove(recipe);

            await SaveAsync();
            _logger.LogInformation("Removed recipe from favorites: {Title}", recipe.Title);
            return true;
        }
    }
}