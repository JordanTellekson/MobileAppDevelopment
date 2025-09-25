using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(Recipe), "Recipe")]
    public class UpdateRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<UpdateRecipeViewModel> _logger;

        public UpdateRecipeViewModel(
            IRecipeRepository repository,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<UpdateRecipeViewModel> logger)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            SaveCommand = new AsyncRelayCommand(OnSaveAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
        }

        private Recipe _recipe;
        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnPropertyChanged();

                if (_recipe != null)
                {
                    _logger.LogInformation("Loaded recipe for update: {Title}", _recipe.Title);

                    Title = _recipe.Title;
                    Description = _recipe.Description;
                    ImageUrl = _recipe.ImageUrl;
                    CookingTimeMinutes = _recipe.CookingTimeMinutes.ToString();
                    Ingredients = string.Join(", ", _recipe.Ingredients ?? new List<string>());
                    Instructions = _recipe.Instructions;
                }
            }
        }

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

        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }

        private async Task OnSaveAsync()
        {
            if (Recipe == null)
            {
                _logger.LogWarning("Save attempted but no recipe loaded");
                await _dialogService.ShowAlertAsync("Error", "No recipe loaded to update", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                _logger.LogWarning("Save attempted with empty title for recipe: {Id}", Recipe.Id);
                await _dialogService.ShowAlertAsync("Error", "Title is required", "OK");
                return;
            }

            // Update recipe properties
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

            _logger.LogInformation("Updating recipe: {Title}", Recipe.Title);

            try
            {
                await _repository.UpdateRecipeAsync(Recipe);
                _logger.LogDebug("Recipe updated successfully: {Title}", Recipe.Title);

                await _navigationService.GoBackAsync();
                _logger.LogDebug("Navigated back after updating recipe: {Title}", Recipe.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe: {Title}", Recipe.Title);
                await _dialogService.ShowAlertAsync("Error", "Failed to update recipe", "OK");
            }
        }

        private async Task OnCancelAsync()
        {
            _logger.LogInformation("Update canceled for recipe: {Title}", Recipe?.Title ?? "null");
            await _navigationService.GoBackAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}