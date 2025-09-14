using Microsoft.Maui.Controls;
using MvvmHelpers;
using RecipeApp.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace RecipeApp.ViewModels
{
    public class AddRecipeViewModel : BaseViewModel
    {
        // Form fields
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string CookingTimeMinutes { get; set; } // keep as string for easy binding
        public string Ingredients { get; set; }
        public string Instructions { get; set; }

        public ICommand SaveRecipeCommand { get; }

        public AddRecipeViewModel()
        {
            SaveRecipeCommand = new Command(OnSaveRecipe);
        }

        private async void OnSaveRecipe()
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(Title))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Title is required", "OK");
                return;
            }

            // Convert CookingTimeMinutes to int
            int cookingTime = 0;
            if (!int.TryParse(CookingTimeMinutes, out cookingTime))
                cookingTime = 0;

            // Convert Ingredients string to list
            var ingredientList = new List<string>();
            if (!string.IsNullOrWhiteSpace(Ingredients))
            {
                foreach (var ing in Ingredients.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(ing))
                        ingredientList.Add(ing.Trim());
                }
            }

            // Create new recipe
            var newRecipe = new Recipe
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl,
                CookingTimeMinutes = cookingTime,
                Ingredients = ingredientList,
                Instructions = Instructions,
                Author = RecipeListViewModel.CurrentAuthor
            };

            // Add to the shared static collection
            RecipeListViewModel.RecipesStore.Add(newRecipe);

            // Navigate back to the list page
            await Shell.Current.GoToAsync("..");
        }
    }
}