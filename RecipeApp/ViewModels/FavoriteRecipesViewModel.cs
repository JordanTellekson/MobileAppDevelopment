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
        private readonly IFavoriteService _favoriteService;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<FavoriteRecipesViewModel> _logger;

        public FavoriteRecipesViewModel(
            IFavoriteService favoriteService,
            IUserService userService,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<FavoriteRecipesViewModel> logger)
        {
            _favoriteService = favoriteService;
            _userService = userService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            Favorites = new ObservableCollection<Recipe>();

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
                Favorites.Clear();

                // ✅ Get current user ID
                var currentUserId = _userService.CurrentUserId;
                if (currentUserId == null || currentUserId == Guid.Empty)
                {
                    await _dialogService.ShowAlertAsync("Error", "User not logged in", "OK");
                    return;
                }

                // ✅ Get the user's favorites from API
                var favorites = await _favoriteService.GetUserFavoritesAsync(currentUserId);
                foreach (var recipe in favorites)
                    Favorites.Add(recipe);

                _logger.LogInformation("Loaded {Count} favorite recipes for user {UserId}", Favorites.Count, currentUserId);
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
                var currentUserId = _userService.CurrentUserId;
                if (currentUserId == null || currentUserId == Guid.Empty)
                {
                    await _dialogService.ShowAlertAsync("Error", "User not logged in", "OK");
                    return;
                }

                bool removed = await _favoriteService.RemoveFavoriteAsync(currentUserId, recipe.Id);

                if (removed)
                {
                    Favorites.Remove(recipe);
                    recipe.IsFavorite = false;

                    await _dialogService.ShowAlertAsync("Removed", $"{recipe.Title} removed from favorites.", "OK");
                    _logger.LogInformation("Removed recipe from favorites: {Title}", recipe.Title);
                }
                else
                {
                    _logger.LogWarning("Attempted to remove recipe that was not a favorite: {Title}", recipe.Title);
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