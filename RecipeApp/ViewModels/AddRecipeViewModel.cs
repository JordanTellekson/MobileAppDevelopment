using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public class AddRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<AddRecipeViewModel> _logger;

        public AddRecipeViewModel(
            IRecipeService recipeService,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<AddRecipeViewModel> logger)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            Categories = new ObservableCollection<Category>();
            SaveRecipeCommand = new AsyncRelayCommand(OnSaveRecipeAsync);
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing AddRecipeViewModel...");

            // Ensure service is initialized
            await _recipeService.InitializeAsync();

            // Populate categories for Picker
            Categories.Clear();
            foreach (var c in _recipeService.Categories)
            {
                Categories.Add(c);
            }

            _logger.LogInformation("Loaded {Count} categories for AddRecipePage.", Categories.Count);
        }

        // ---------------------------
        // Recipe fields
        // ---------------------------
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _imageUrl;
        public string ImageUrl { get => _imageUrl; set { _imageUrl = value; OnPropertyChanged(); } }

        private string _cookingTimeMinutes;
        public string CookingTimeMinutes { get => _cookingTimeMinutes; set { _cookingTimeMinutes = value; OnPropertyChanged(); } }

        private string _ingredients;
        public string Ingredients { get => _ingredients; set { _ingredients = value; OnPropertyChanged(); } }

        private string _instructions;
        public string Instructions { get => _instructions; set { _instructions = value; OnPropertyChanged(); } }

        // ---------------------------
        // Categories for Picker
        // ---------------------------
        public ObservableCollection<Category> Categories { get; }

        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        // ---------------------------
        // Commands
        // ---------------------------
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
                ingredientList = Ingredients
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .ToList();
            }

            var newRecipe = new Recipe
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl,
                CookingTimeMinutes = cookingTime,
                Ingredients = ingredientList,
                Instructions = Instructions,
                Author = _userService.CurrentUser,
                CategoryId = SelectedCategory?.Id ?? Guid.Empty,
                CategoryName = SelectedCategory?.Name
            };

            try
            {
                _logger.LogInformation("Adding recipe: {Title} by {Author}", newRecipe.Title, newRecipe.Author);
                await _recipeService.AddRecipeAsync(newRecipe);
                _logger.LogInformation("Recipe added successfully: {Title}", newRecipe.Title);

                // Clear input fields
                Title = Description = ImageUrl = CookingTimeMinutes = Ingredients = Instructions = string.Empty;
                SelectedCategory = null;

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