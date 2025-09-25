using RecipeApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RecipeApp.Services
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly ILogger<RecipeRepository> _logger;

        public RecipeRepository(ILogger<RecipeRepository> logger)
        {
            _logger = logger;

            // Seed sample data
            Recipes.Add(new Recipe
            {
                Id = Guid.NewGuid(),
                Title = "Spaghetti Carbonara",
                Description = "Classic Italian pasta dish.",
                ImageUrl = "spaghetti.jpg",
                Ingredients = new List<string> { "Pasta", "Eggs", "Pancetta", "Parmesan" },
                Instructions = "Cook pasta, fry pancetta, mix with eggs and cheese.",
                CookingTimeMinutes = 30,
                Author = "SomeoneElse"
            });

            Recipes.Add(new Recipe
            {
                Id = Guid.NewGuid(),
                Title = "Chicken Curry",
                Description = "Aromatic and spicy curry.",
                ImageUrl = "curry.jpg",
                Ingredients = new List<string> { "Chicken", "Curry Paste", "Coconut Milk" },
                Instructions = "Cook chicken, add curry paste, stir in coconut milk.",
                CookingTimeMinutes = 40,
                Author = "OtherUser"
            });
        }

        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
        public ObservableCollection<Recipe> Favorites { get; } = new ObservableCollection<Recipe>();

        public Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));

            if (recipe.Id == Guid.Empty)
                recipe.Id = Guid.NewGuid();

            Recipes.Add(recipe);
            _logger.LogInformation("Recipe added: {Title} by {Author}", recipe.Title, recipe.Author);

            return Task.CompletedTask;
        }

        public Task UpdateRecipeAsync(Recipe recipe)
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

                _logger.LogInformation("Recipe updated: {Title} by {Author}", recipe.Title, recipe.Author);
            }
            else
            {
                _logger.LogWarning("Update failed: Recipe with Id {Id} not found", recipe.Id);
            }

            return Task.CompletedTask;
        }

        public Task<Recipe?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = Recipes.FirstOrDefault(r => r.Id == id);
            _logger.LogDebug("GetRecipeByIdAsync called for Id {Id}. Found: {Found}", id, recipe != null);
            return Task.FromResult(recipe);
        }

        public Task DeleteRecipeAsync(Guid id)
        {
            var recipe = Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe != null)
            {
                Recipes.Remove(recipe);
                _logger.LogInformation("Recipe deleted: {Title} by {Author}", recipe.Title, recipe.Author);
            }
            else
            {
                _logger.LogWarning("Delete failed: Recipe with Id {Id} not found", id);
            }

            return Task.CompletedTask;
        }

        public bool AddToFavorites(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("AddToFavorites called with null recipe");
                return false;
            }

            if (Favorites.Any(r => r.Id == recipe.Id))
            {
                _logger.LogInformation("Recipe {Title} is already a favorite", recipe.Title);
                return false;
            }

            Favorites.Add(recipe);
            _logger.LogInformation("Recipe added to favorites: {Title}", recipe.Title);
            return true;
        }

        public bool RemoveFromFavorites(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("RemoveFromFavorites called with null recipe");
                return false;
            }

            bool removed = Favorites.Remove(recipe);
            if (removed)
                _logger.LogInformation("Recipe removed from favorites: {Title}", recipe.Title);
            else
                _logger.LogWarning("Attempted to remove recipe {Title} from favorites, but it was not found", recipe.Title);

            return removed;
        }
    }
}