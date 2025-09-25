using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public class FavoriteRecipesViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<FavoriteRecipesViewModel> _logger;

        public FavoriteRecipesViewModel(
            IRecipeRepository repository,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<FavoriteRecipesViewModel> logger)
        {
            _repository = repository;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            // Bind directly to repository's Favorites collection
            Favorites = _repository.Favorites;

            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            RemoveFromFavoritesCommand = new RelayCommand<Recipe>(OnRemoveFromFavorites);
        }

        public ObservableCollection<Recipe> Favorites { get; }

        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IRelayCommand<Recipe> RemoveFromFavoritesCommand { get; }

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

        private async void OnRemoveFromFavorites(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("RemoveFromFavoritesCommand called with null recipe");
                return;
            }

            if (_repository.RemoveFromFavorites(recipe))
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}