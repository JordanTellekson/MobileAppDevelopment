using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Services;
using RecipeApp.Repositories;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(RecipeId), "RecipeId")]
    public class RecipeDetailViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<RecipeDetailViewModel> _logger;

        public RecipeDetailViewModel(
            IRecipeRepository repository,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<RecipeDetailViewModel> logger)
        {
            _repository = repository;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            ToggleFavoriteCommand = new AsyncRelayCommand(OnToggleFavoriteAsync);
        }

        private Recipe _recipe;
        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CategoryDisplay)); // update when recipe changes
            }
        }

        private string _recipeId;
        public string RecipeId
        {
            get => _recipeId;
            set
            {
                _recipeId = value;
                _logger.LogInformation("Recipe ID set for detail view: {RecipeId}", _recipeId);
                _ = LoadRecipeAsync();
            }
        }

        public IAsyncRelayCommand ToggleFavoriteCommand { get; }

        public string CategoryDisplay => string.IsNullOrWhiteSpace(Recipe?.CategoryName)
            ? "Uncategorized"
            : $"Category: {Recipe.CategoryName}";

        private async Task LoadRecipeAsync()
        {
            _logger.LogDebug("Attempting to load recipe with ID: {RecipeId}", RecipeId);

            if (Guid.TryParse(RecipeId, out var id))
            {
                try
                {
                    Recipe = await _repository.GetRecipeByIdAsync(id);

                    if (Recipe == null)
                    {
                        _logger.LogWarning("Recipe not found with ID: {RecipeId}", RecipeId);
                        await _dialogService.ShowAlertAsync("Error", "Recipe not found", "OK");
                        await _navigationService.GoBackAsync();
                    }
                    else
                    {
                        _logger.LogInformation("Loaded recipe: {Title}", Recipe.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading recipe with ID: {RecipeId}", RecipeId);
                    await _dialogService.ShowAlertAsync("Error", "Failed to load recipe", "OK");
                    await _navigationService.GoBackAsync();
                }
            }
            else
            {
                _logger.LogWarning("Invalid Recipe ID provided: {RecipeId}", RecipeId);
                await _dialogService.ShowAlertAsync("Error", "Invalid Recipe ID", "OK");
                await _navigationService.GoBackAsync();
            }
        }

        private async Task OnToggleFavoriteAsync()
        {
            if (Recipe == null)
            {
                _logger.LogWarning("ToggleFavoriteCommand called with null recipe");
                return;
            }

            try
            {
                if (Recipe.IsFavorite)
                {
                    bool removed = await _repository.RemoveFromFavoritesAsync(Recipe);
                    if (removed)
                    {
                        Recipe.IsFavorite = false;
                        _logger.LogInformation("Removed recipe from favorites: {Title}", Recipe.Title);
                    }
                }
                else
                {
                    bool added = await _repository.AddToFavoritesAsync(Recipe);
                    if (added)
                    {
                        Recipe.IsFavorite = true;
                        _logger.LogInformation("Added recipe to favorites: {Title}", Recipe.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite status for recipe: {Title}", Recipe.Title);
                await _dialogService.ShowAlertAsync("Error", "Failed to update favorites", "OK");
            }

            OnPropertyChanged(nameof(Recipe));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}