using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using RecipeApp.Api.Data;

namespace RecipeApp.Repositories
{
    public class ApiRecipeRepository : IRecipeRepository
    {
        private readonly ILogger<ApiRecipeRepository> _logger;
        private readonly AppDbContext _dbContext;

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public ApiRecipeRepository(ILogger<ApiRecipeRepository> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        // ---------------------------
        // Initialization
        // ---------------------------
        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing SQL RecipeRepository (combined)...");

            // Load categories as no-tracking for the observable collection
            var categories = await _dbContext.Categories
                                             .AsNoTracking()
                                             .ToListAsync();

            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);

            // Load recipes including categories normally (tracking)
            var recipes = await _dbContext.Recipes.Include(r => r.Category).ToListAsync();
            Recipes.Clear();
            Favorites.Clear();

            foreach (var r in recipes)
            {
                Recipes.Add(r);
                if (r.IsFavorite)
                    Favorites.Add(r);
            }

            _logger.LogInformation("Loaded {Count} recipes and {CatCount} categories from SQL.", Recipes.Count, Categories.Count);
        }

        // ---------------------------
        // CATEGORY CRUD
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

            try
            {
                // Detach any existing tracked entity with the same Id
                var tracked = _dbContext.ChangeTracker.Entries<Category>()
                                            .FirstOrDefault(e => e.Entity.Id == category.Id);
                if (tracked != null)
                    tracked.State = EntityState.Detached;

                // Attach the incoming entity and mark as modified
                _dbContext.Categories.Attach(category);
                _dbContext.Entry(category).Property(c => c.Name).IsModified = true;

                await _dbContext.SaveChangesAsync();

                // Update observable collection
                var obs = Categories.FirstOrDefault(c => c.Id == category.Id);
                if (obs != null)
                    obs.Name = category.Name;

                _logger.LogInformation("Updated category: {Name}", category.Name);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to update category {Name}", category.Name);
                throw;
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

            if (category == null)
            {
                _logger.LogWarning("Delete failed: Category with Id {Id} not found", id);
                return;
            }

            // Detach category from any recipes first
            var recipesWithCategory = await _dbContext.Recipes
                                        .Where(r => r.CategoryId == id)
                                        .ToListAsync();

            foreach (var recipe in recipesWithCategory)
            {
                recipe.CategoryId = null;
                recipe.Category = null;
            }

            // Save recipe changes before removing the category
            await _dbContext.SaveChangesAsync();

            // Remove the category
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            // Update observable collection
            Categories.Remove(category);

            _logger.LogInformation("Deleted category: {Name}", category.Name);
        }

        // ---------------------------
        // RECIPE CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (recipe.Id == Guid.Empty) recipe.Id = Guid.NewGuid();

            try
            {
                // Attach category properly
                if (recipe.Category != null)
                {
                    if (recipe.Category.Id == Guid.Empty)
                        recipe.Category.Id = Guid.NewGuid();

                    // Make sure EF doesn’t try to insert duplicate category
                    _dbContext.Entry(recipe.Category).State = EntityState.Unchanged;
                }

                await _dbContext.Recipes.AddAsync(recipe);
                await _dbContext.SaveChangesAsync();

                Recipes.Add(recipe);
                if (recipe.IsFavorite)
                    Favorites.Add(recipe);

                _logger.LogInformation("✅ Added recipe: {Title}", recipe.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to add recipe: {Title}", recipe.Title);
                throw; // Let controller handle HTTP 500, but now we’ll see the real cause
            }
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            var existing = await _dbContext.Recipes.Include(r => r.Category)
                                                   .FirstOrDefaultAsync(r => r.Id == recipe.Id);
            if (existing != null)
            {
                _dbContext.Entry(existing).CurrentValues.SetValues(recipe);

                // Update category relationship
                existing.CategoryId = recipe.CategoryId;

                await _dbContext.SaveChangesAsync();

                // Update observable collection
                var obs = Recipes.First(r => r.Id == recipe.Id);
                obs.Title = recipe.Title;
                obs.Description = recipe.Description;
                obs.ImageUrl = recipe.ImageUrl;
                obs.CookingTimeMinutes = recipe.CookingTimeMinutes;
                obs.Ingredients = recipe.Ingredients;
                obs.Instructions = recipe.Instructions;
                obs.Author = recipe.Author;
                obs.CategoryId = recipe.CategoryId;
                obs.Category = recipe.Category;
                obs.IsFavorite = recipe.IsFavorite;

                if (recipe.IsFavorite && !Favorites.Any(r => r.Id == recipe.Id))
                    Favorites.Add(obs);
                else if (!recipe.IsFavorite)
                    Favorites.Remove(obs);

                _logger.LogInformation("Updated recipe: {Title}", recipe.Title);
            }
            else
            {
                _logger.LogWarning("Update failed: Recipe with Id {Id} not found", recipe.Id);
            }
        }

        public async Task<Recipe?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = await _dbContext.Recipes.Include(r => r.Category)
                                                 .FirstOrDefaultAsync(r => r.Id == id);
            _logger.LogDebug("GetRecipeByIdAsync({Id}) -> Found: {Found}", id, recipe != null);
            return recipe;
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            var recipe = await _dbContext.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _dbContext.Recipes.Remove(recipe);
                await _dbContext.SaveChangesAsync();

                Recipes.Remove(recipe);
                Favorites.Remove(recipe);

                _logger.LogInformation("Deleted recipe: {Title}", recipe.Title);
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

            _dbContext.Recipes.Update(recipe);
            await _dbContext.SaveChangesAsync();

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

            _dbContext.Recipes.Update(recipe);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Removed recipe from favorites: {Title}", recipe.Title);
            return true;
        }
    }
}