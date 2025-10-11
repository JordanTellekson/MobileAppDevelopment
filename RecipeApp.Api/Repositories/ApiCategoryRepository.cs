using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using RecipeApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Repositories
{
    public class ApiCategoryRepository : ICategoryRepository
    {
        private readonly ILogger<ApiCategoryRepository> _logger;
        private readonly AppDbContext _dbContext;

        public ObservableCollection<Category> Categories { get; } = new();

        public ApiCategoryRepository(ILogger<ApiCategoryRepository> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing SQL CategoryRepository...");

            var categories = await _dbContext.Categories.ToListAsync();
            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);

            _logger.LogInformation("Loaded {Count} categories from SQL.", Categories.Count);
        }

        // ---------------------------
        // CRUD
        // ---------------------------
        public async Task AddCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));
            if (category.Id == Guid.Empty) category.Id = Guid.NewGuid();

            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();

            Categories.Add(category);
            _logger.LogInformation("Added category: {Name}", category.Name);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category));

            var existing = await _dbContext.Categories.FindAsync(category.Id);
            if (existing != null)
            {
                _dbContext.Entry(existing).CurrentValues.SetValues(category);
                await _dbContext.SaveChangesAsync();

                var obs = Categories.First(c => c.Id == category.Id);
                obs.Name = category.Name;

                _logger.LogInformation("Updated category: {Name}", category.Name);
            }
            else
            {
                _logger.LogWarning("Update failed: Category with Id {Id} not found", category.Id);
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            _logger.LogDebug("GetCategoryByIdAsync({Id}) -> Found: {Found}", id, category != null);
            return category;
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category != null)
            {
                _dbContext.Categories.Remove(category);
                await _dbContext.SaveChangesAsync();

                Categories.Remove(category);
                _logger.LogInformation("Deleted category: {Name}", category.Name);
            }
            else
            {
                _logger.LogWarning("Delete failed: Category with Id {Id} not found", id);
            }
        }
    }
}