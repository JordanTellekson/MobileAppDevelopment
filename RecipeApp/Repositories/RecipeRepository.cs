using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace RecipeApp.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ILogger<RecipeRepository> _logger;
        private readonly string _localFilePath;
        private JsonDataStore _dataStore = new(); // Holds Recipes + Categories

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public RecipeRepository(ILogger<RecipeRepository> logger)
        {
            _logger = logger;
            _localFilePath = Path.Combine(FileSystem.AppDataDirectory, "recipes.json");
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing RecipeRepository...");

                if (!File.Exists(_localFilePath))
                {
                    _logger.LogWarning("Local JSON not found at {Path}. Copying seed data...", _localFilePath);
                    await CopySeedDataAsync();
                }

                string json = await File.ReadAllTextAsync(_localFilePath);
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
                    // Link category object
                    if (r.CategoryId.HasValue)
                    {
                        r.Category = Categories.FirstOrDefault(c => c.Id == r.CategoryId.Value);
                    }

                    Recipes.Add(r);
                    if (r.IsFavorite)
                        Favorites.Add(r);
                }

                _logger.LogInformation("Loaded {Count} recipes and {CatCount} categories.", Recipes.Count, Categories.Count);
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

        private async Task SaveDataStoreAsync()
        {
            try
            {
                // Ensure CategoryId is updated from Category reference
                foreach (var r in _dataStore.Recipes)
                {
                    r.CategoryId = r.Category?.Id;
                }

                string json = JsonSerializer.Serialize(_dataStore, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_localFilePath, json);
                _logger.LogInformation("Recipes and categories saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save data store to JSON.");
            }
        }

        // ---------------------------
        // Recipe CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (recipe.Id == Guid.Empty) recipe.Id = Guid.NewGuid();

            _dataStore.Recipes.Add(recipe);
            Recipes.Add(recipe);
            if (recipe.IsFavorite) Favorites.Add(recipe);

            _logger.LogInformation("Added recipe: {Title}", recipe.Title);
            await SaveDataStoreAsync();
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            var existing = _dataStore.Recipes.FirstOrDefault(r => r.Id == recipe.Id);
            if (existing != null)
            {
                // Update stored recipe
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

                // Update observable collection
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
                await SaveDataStoreAsync();
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
                await SaveDataStoreAsync();
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
            await SaveDataStoreAsync();
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
                await SaveDataStoreAsync();
            }
            else
            {
                _logger.LogWarning("Attempted to remove recipe {Title} from favorites, but not found", recipe.Title);
            }

            return removed;
        }
    }
}