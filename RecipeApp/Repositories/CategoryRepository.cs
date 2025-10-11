using RecipeApp.Shared.Models;
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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ILogger<CategoryRepository> _logger;
        private readonly string _localFilePath;
        private JsonDataStore _dataStore = new();

        public ObservableCollection<Category> Categories { get; } = new();

        public CategoryRepository(ILogger<CategoryRepository> logger)
        {
            _logger = logger;
            _localFilePath = Path.Combine(FileSystem.AppDataDirectory, "recipes.json");
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing CategoryRepository...");

                if (!File.Exists(_localFilePath))
                {
                    _logger.LogWarning("Local JSON not found at {Path}. Copying seed data...", _localFilePath);
                    await CopySeedDataAsync();
                }

                string json = await File.ReadAllTextAsync(_localFilePath);
                _dataStore = JsonSerializer.Deserialize<JsonDataStore>(json) ?? new JsonDataStore();

                Categories.Clear();
                foreach (var c in _dataStore.Categories)
                    Categories.Add(c);

                _logger.LogInformation("Loaded {Count} categories.", Categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize CategoryRepository.");
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
                string json = JsonSerializer.Serialize(_dataStore, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_localFilePath, json);
                _logger.LogInformation("Categories saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save data store to JSON.");
            }
        }

        // ---------------------------
        // CRUD
        // ---------------------------
        public async Task AddCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (category.Id == Guid.Empty) category.Id = Guid.NewGuid();

            _dataStore.Categories.Add(category);
            Categories.Add(category);

            _logger.LogInformation("Added category: {Name}", category.Name);
            await SaveDataStoreAsync();
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
                await SaveDataStoreAsync();
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

                _logger.LogInformation("Deleted category: {Name}", category.Name);
                await SaveDataStoreAsync();
            }
            else
            {
                _logger.LogWarning("Delete failed: Category with Id {Id} not found", id);
            }
        }
    }
}