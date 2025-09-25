using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public class AddRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<AddRecipeViewModel> _logger;

        public AddRecipeViewModel(
            IRecipeRepository repository,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<AddRecipeViewModel> logger)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            SaveRecipeCommand = new AsyncRelayCommand(OnSaveRecipeAsync);
        }

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get => _imageUrl;
            set { _imageUrl = value; OnPropertyChanged(); }
        }

        private string _cookingTimeMinutes;
        public string CookingTimeMinutes
        {
            get => _cookingTimeMinutes;
            set { _cookingTimeMinutes = value; OnPropertyChanged(); }
        }

        private string _ingredients;
        public string Ingredients
        {
            get => _ingredients;
            set { _ingredients = value; OnPropertyChanged(); }
        }

        private string _instructions;
        public string Instructions
        {
            get => _instructions;
            set { _instructions = value; OnPropertyChanged(); }
        }

        public IAsyncRelayCommand SaveRecipeCommand { get; }

        private async Task OnSaveRecipeAsync()
        {
            _logger.LogInformation("SaveRecipeCommand triggered");

            if (string.IsNullOrWhiteSpace(Title))
            {
                _logger.LogWarning("Save attempted with empty Title");
                await _dialogService.ShowAlertAsync("Error", "Title is required", "OK");
                return;
            }

            int cookingTime = int.TryParse(CookingTimeMinutes, out var minutes) ? minutes : 0;

            var ingredientList = new List<string>();
            if (!string.IsNullOrWhiteSpace(Ingredients))
            {
                foreach (var ing in Ingredients.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(ing))
                        ingredientList.Add(ing.Trim());
                }
            }

            var newRecipe = new Recipe
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl,
                CookingTimeMinutes = cookingTime,
                Ingredients = ingredientList,
                Instructions = Instructions,
                Author = _userService.CurrentUser
            };

            try
            {
                _logger.LogInformation("Adding recipe: {Title} by {Author}", newRecipe.Title, newRecipe.Author);
                await _repository.AddRecipeAsync(newRecipe);
                _logger.LogInformation("Recipe added successfully: {Title}", newRecipe.Title);

                await _navigationService.GoBackAsync();
                _logger.LogDebug("Navigation back after adding recipe completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving recipe: {Title}", newRecipe.Title);
                await _dialogService.ShowAlertAsync("Error", "Failed to save recipe", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}