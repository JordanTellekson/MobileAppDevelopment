using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace RecipeApp.Services
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ILogger<RecipeRepository> _logger;
        private readonly string _localFilePath;

        // Keep these collections constant so UI bindings remain valid
        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();

        public RecipeRepository(ILogger<RecipeRepository> logger)
        {
            _logger = logger;
            _localFilePath = Path.Combine(FileSystem.AppDataDirectory, "recipes.json");
        }

        // Make this public so viewmodels can await it
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing RecipeRepository...");

                // Check if local file exists
                if (!File.Exists(_localFilePath))
                {
                    _logger.LogWarning("Local JSON not found at {Path}. Copying seed data...", _localFilePath);
                    await CopySeedDataAsync();
                }
                else
                {
                    _logger.LogInformation("Local JSON found at {Path}.", _localFilePath);
                }

                // Read the JSON file
                string json = await File.ReadAllTextAsync(_localFilePath);
                _logger.LogInformation("JSON content length: {Length}", json?.Length ?? 0);
                _logger.LogDebug("JSON content preview: {JsonPreview}", json?.Substring(0, Math.Min(200, json.Length)));

                // Deserialize
                var loadedRecipes = JsonSerializer.Deserialize<List<Recipe>>(json);
                if (loadedRecipes == null)
                {
                    _logger.LogWarning("Deserialization returned null. Check JSON format.");
                    return;
                }

                // Populate ObservableCollection
                Recipes.Clear();
                foreach (var r in loadedRecipes)
                {
                    Recipes.Add(r);
                }

                _logger.LogInformation("Loaded {Count} recipes into Recipes collection.", Recipes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize recipe repository.");
            }
        }

        private async Task CopySeedDataAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("Data/recipes.json");
                using var reader = new StreamReader(stream);
                string seedJson = await reader.ReadToEndAsync();
                await File.WriteAllTextAsync(_localFilePath, seedJson);

                _logger.LogInformation("Seed data copied to local app directory.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to copy seed data from JSON.");
            }
        }

        private async Task LoadRecipesAsync()
        {
            try
            {
                if (!File.Exists(_localFilePath))
                {
                    _logger.LogWarning("No JSON file found at: {Path}", _localFilePath);
                    return;
                }

                string json = await File.ReadAllTextAsync(_localFilePath);
                var loadedRecipes = JsonSerializer.Deserialize<ObservableCollection<Recipe>>(json);

                Recipes.Clear();
                if (loadedRecipes != null)
                {
                    foreach (var r in loadedRecipes)
                        Recipes.Add(r);

                    _logger.LogInformation("Loaded {Count} recipes from JSON.", Recipes.Count);
                }
                else
                {
                    _logger.LogWarning("No recipes were loaded (empty JSON).");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipes from JSON.");
            }
        }

        private async Task SaveRecipesAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(Recipes, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_localFilePath, json);
                _logger.LogInformation("Recipes saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save recipes to JSON.");
            }
        }

        // ---------------------------
        // CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (recipe.Id == Guid.Empty) recipe.Id = Guid.NewGuid();

            Recipes.Add(recipe);
            _logger.LogInformation("Added recipe: {Title} by {Author}", recipe.Title, recipe.Author);
            await SaveRecipesAsync();
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            var existing = Recipes.FirstOrDefault(r => r.Id == recipe.Id);
            if (existing != null)
            {
                existing.Title = recipe.Title;
                existing.Description = recipe.Description;
                existing.ImageUrl = recipe.ImageUrl;
                existing.CookingTimeMinutes = recipe.CookingTimeMinutes;
                existing.Ingredients = recipe.Ingredients;
                existing.Instructions = recipe.Instructions;
                existing.Author = recipe.Author;

                _logger.LogInformation("Updated recipe: {Title} by {Author}", recipe.Title, recipe.Author);
                await SaveRecipesAsync();
            }
            else
            {
                _logger.LogWarning("Update failed: Recipe with Id {Id} not found", recipe.Id);
            }
        }

        public Task<Recipe?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = Recipes.FirstOrDefault(r => r.Id == id);
            _logger.LogDebug("GetRecipeByIdAsync({Id}) -> Found: {Found}", id, recipe != null);
            return Task.FromResult(recipe);
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            var recipe = Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe != null)
            {
                Recipes.Remove(recipe);
                _logger.LogInformation("Deleted recipe: {Title}", recipe.Title);
                await SaveRecipesAsync();
            }
            else
            {
                _logger.LogWarning("Delete failed: Recipe with Id {Id} not found", id);
            }
        }

        // ---------------------------
        // Favorites
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

            Favorites.Add(recipe);
            recipe.IsFavorite = true;
            _logger.LogInformation("Added recipe to favorites: {Title}", recipe.Title);
            await SaveRecipesAsync();
            return true;
        }

        public async Task<bool> RemoveFromFavoritesAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("RemoveFromFavoritesAsync called with null recipe");
                return false;
            }

            bool removed = Favorites.Remove(recipe);
            recipe.IsFavorite = false;

            if (removed)
            {
                _logger.LogInformation("Removed recipe from favorites: {Title}", recipe.Title);
                await SaveRecipesAsync();
            }
            else
            {
                _logger.LogWarning("Attempted to remove recipe {Title} from favorites, but not found", recipe.Title);
            }

            return removed;
        }
    }
}