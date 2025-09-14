using Microsoft.Maui.Controls;
using MvvmHelpers;
using RecipeApp.Models;
using RecipeApp.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RecipeApp.ViewModels
{
    public class RecipeListViewModel : BaseViewModel
    {
        // Static shared collection for in-memory storage
        public static ObservableCollection<Recipe> RecipesStore { get; set; }

        // Instance property for binding in XAML
        public ObservableCollection<Recipe> Recipes => RecipesStore;

        public ICommand RecipeTappedCommand { get; }

        public ICommand AddRecipeCommand { get; }

        public ICommand UpdateRecipeCommand { get; }

        // Local author "Test" for testing purposes
        public static string CurrentAuthor { get; set; } = "Test";

        public RecipeListViewModel()
        {
            // Initialize the static collection only once
            if (RecipesStore == null)
            {
                RecipesStore = new ObservableCollection<Recipe>
                {
                    new Recipe
                    {
                        Title = "Spaghetti Carbonara",
                        Description = "Classic Italian pasta dish.",
                        ImageUrl = "spaghetti.jpg",
                        Ingredients = new List<string>{ "Pasta", "Eggs", "Pancetta", "Parmesan" },
                        Instructions = "Cook pasta, fry pancetta, mix with eggs and cheese.",
                        CookingTimeMinutes = 30,
                        Author = "SomeoneElse"
                    },
                    new Recipe
                    {
                        Title = "Chicken Curry",
                        Description = "Aromatic and spicy curry.",
                        ImageUrl = "curry.jpg",
                        Ingredients = new List<string>{ "Chicken", "Curry Paste", "Coconut Milk" },
                        Instructions = "Cook chicken, add curry paste, stir in coconut milk.",
                        CookingTimeMinutes = 40,
                        Author = "OtherUser"
                    }
                };
            }

            RecipeTappedCommand = new Command<Recipe>(OnRecipeTapped);
            AddRecipeCommand = new Command(OnAddRecipe);
            UpdateRecipeCommand = new Command<Recipe>(OnUpdateRecipe);
        }

        // Navigate to recipe detail page, passing the selected recipe via Shell parameters
        private async void OnRecipeTapped(Recipe recipe)
        {
            if (recipe == null)
                return;

            await Shell.Current.GoToAsync(nameof(RecipeDetailPage), new Dictionary<string, object>
            {
                { "Recipe", recipe }
            });
        }

        // Navigate to add recipe page
        private async void OnAddRecipe()
        {
            await Shell.Current.GoToAsync(nameof(AddRecipePage));
        }

        // Navigate to update recipe page, passing the selected recipe via Shell parameters
        private async void OnUpdateRecipe(Recipe recipe)
        {
            if (recipe == null)
                return;

            var navigationParameter = new Dictionary<string, object>
            {
                { "Recipe", recipe }
            };

            await Shell.Current.GoToAsync(nameof(UpdateRecipePage), navigationParameter);
        }
    }
}