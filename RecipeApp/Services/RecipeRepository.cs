using RecipeApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public class RecipeRepository : IRecipeRepository
    {
        // ObservableCollection drives the UI automatically
        public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>();
        public ObservableCollection<Recipe> Favorites { get; } = new ObservableCollection<Recipe>();

        public RecipeRepository()
        {
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

        public Task AddRecipeAsync(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            if (recipe.Id == Guid.Empty)
                recipe.Id = Guid.NewGuid();

            Recipes.Add(recipe);
            return Task.CompletedTask;
        }

        public Task UpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

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
            }

            return Task.CompletedTask;
        }

        public Task<Recipe?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = Recipes.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(recipe);
        }

        public Task DeleteRecipeAsync(Guid id)
        {
            var recipe = Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe != null)
                Recipes.Remove(recipe);

            return Task.CompletedTask;
        }

        public bool AddToFavorites(Recipe recipe)
        {
            if (recipe == null) return false;

            if (Favorites.Any(r => r.Id == recipe.Id))
                return false; // already a favorite

            Favorites.Add(recipe);
            return true;
        }

        public bool RemoveFromFavorites(Recipe recipe)
        {
            if (recipe == null) return false;

            return Favorites.Remove(recipe);
        }
    }
}