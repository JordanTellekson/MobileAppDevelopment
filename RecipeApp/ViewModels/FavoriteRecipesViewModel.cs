using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public class FavoriteRecipesViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeService _recipeService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<FavoriteRecipesViewModel> _logger;

        public FavoriteRecipesViewModel(
            IRecipeService recipeService,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<FavoriteRecipesViewModel> logger)
        {
            _recipeService = recipeService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            Favorites = _recipeService.Favorites;

            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            RemoveFromFavoritesCommand = new AsyncRelayCommand<Recipe>(OnRemoveFromFavoritesAsync);
        }

        public ObservableCollection<Recipe> Favorites { get; }

        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IAsyncRelayCommand<Recipe> RemoveFromFavoritesCommand { get; }

        public async Task InitializeAsync()
        {
            try
            {
                await _recipeService.InitializeAsync();
                _logger.LogInformation("Favorite recipes loaded. Count: {Count}", Favorites.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize favorites");
                await _dialogService.ShowAlertAsync("Error", "Failed to load favorites", "OK");
            }
        }

        private async Task OnRecipeTappedAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("RecipeTappedCommand called with null recipe");
                return;
            }

            _logger.LogInformation("Recipe tapped: {Title}", recipe.Title);

            var parameters = new Dictionary<string, object>
            {
                { "RecipeId", recipe.Id.ToString() }
            };

            try
            {
                await _navigationService.NavigateToAsync(nameof(Views.RecipeDetailPage), parameters);
                _logger.LogDebug("Navigation to RecipeDetailPage successful for {Title}", recipe.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation failed for recipe: {Title}", recipe.Title);
            }
        }

        private async Task OnRemoveFromFavoritesAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("RemoveFromFavoritesCommand called with null recipe");
                return;
            }

            try
            {
                bool removed = await _recipeService.RemoveFromFavoritesAsync(recipe);

                if (removed)
                {
                    _logger.LogInformation("Removed recipe from favorites: {Title}", recipe.Title);
                    recipe.IsFavorite = false;

                    try
                    {
                        await _dialogService.ShowAlertAsync("Removed", $"{recipe.Title} removed from favorites.", "OK");
                        _logger.LogDebug("Alert shown for removing recipe: {Title}", recipe.Title);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to show alert after removing recipe: {Title}", recipe.Title);
                    }
                }
                else
                {
                    _logger.LogWarning("Attempted to remove recipe from favorites but it was not found: {Title}", recipe.Title);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove recipe from favorites: {Title}", recipe.Title);
                await _dialogService.ShowAlertAsync("Error", $"Failed to remove {recipe.Title} from favorites", "OK");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}