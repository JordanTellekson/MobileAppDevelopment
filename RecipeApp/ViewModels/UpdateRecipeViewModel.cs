using Microsoft.Maui.Controls;
using MvvmHelpers;
using RecipeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(Recipe), "Recipe")]
    public class UpdateRecipeViewModel : BaseViewModel
    {
        private Recipe _recipe;

        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnPropertyChanged();

                // Pre-fill editable fields when the recipe is set
                if (_recipe != null)
                {
                    Title = _recipe.Title;
                    Description = _recipe.Description;
                    ImageUrl = _recipe.ImageUrl;
                    CookingTimeMinutes = _recipe.CookingTimeMinutes.ToString();
                    Ingredients = string.Join(", ", _recipe.Ingredients ?? new List<string>());
                    Instructions = _recipe.Instructions;
                }
            }
        }

        // Editable fields
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        private string _cookingTimeMinutes;
        public string CookingTimeMinutes
        {
            get => _cookingTimeMinutes;
            set => SetProperty(ref _cookingTimeMinutes, value);
        }

        private string _ingredients;
        public string Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        private string _instructions;
        public string Instructions
        {
            get => _instructions;
            set => SetProperty(ref _instructions, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public UpdateRecipeViewModel()
        {
            SaveCommand = new Command(OnSave);
            CancelCommand = new Command(OnCancel);
        }

        private async void OnSave()
        {
            if (Recipe == null)
                return;

            // Update the original recipe directly
            Recipe.Title = Title;
            Recipe.Description = Description;
            Recipe.ImageUrl = ImageUrl;
            Recipe.CookingTimeMinutes = int.TryParse(CookingTimeMinutes, out var minutes) ? minutes : 0;
            Recipe.Ingredients = Ingredients?.Split(',')
                                             .Where(i => !string.IsNullOrWhiteSpace(i))
                                             .Select(i => i.Trim())
                                             .ToList()
                                 ?? new List<string>();
            Recipe.Instructions = Instructions;

            // Navigate back
            await Shell.Current.GoToAsync("..");
        }

        private async void OnCancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}