using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing SQL RecipeRepository...");

            var categories = await _dbContext.Categories.ToListAsync();
            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);

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
        // Recipe CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (recipe.Id == Guid.Empty) recipe.Id = Guid.NewGuid();

            // Attach category if it exists
            if (recipe.Category != null)
            {
                _dbContext.Entry(recipe.Category).State = EntityState.Unchanged;
            }

            await _dbContext.Recipes.AddAsync(recipe);
            await _dbContext.SaveChangesAsync();

            Recipes.Add(recipe);
            if (recipe.IsFavorite)
                Favorites.Add(recipe);

            _logger.LogInformation("Added recipe: {Title}", recipe.Title);
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            var existing = await _dbContext.Recipes.Include(r => r.Category)
                                                   .FirstOrDefaultAsync(r => r.Id == recipe.Id);
            if (existing != null)
            {
                // Update scalar properties
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