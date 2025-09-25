using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Models;
using RecipeApp.Resources.Styles;
using RecipeApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RecipeApp.ViewModels
{
    public class RecipeListViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<RecipeListViewModel> _logger;

        public RecipeListViewModel(
            IRecipeRepository repository,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<RecipeListViewModel> logger)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            // Bind directly to the repository's ObservableCollection
            Recipes = _repository.Recipes;

            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            AddRecipeCommand = new AsyncRelayCommand(OnAddRecipeAsync);
            UpdateRecipeCommand = new AsyncRelayCommand<Recipe>(OnUpdateRecipeAsync);
            AddToFavoritesCommand = new RelayCommand<Recipe>(OnAddToFavorites);
            NavigateToFavoritesCommand = new RelayCommand(OnNavigateToFavorites);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
        }

        public ObservableCollection<Recipe> Recipes { get; }
        public string CurrentUser => _userService.CurrentUser;

        private bool _isDarkMode = false;

        public string ThemeButtonText => _isDarkMode ? "Light Mode" : "Dark Mode";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IAsyncRelayCommand AddRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> UpdateRecipeCommand { get; }
        public IRelayCommand<Recipe> AddToFavoritesCommand { get; }
        public IRelayCommand NavigateToFavoritesCommand { get; }
        public IRelayCommand ToggleThemeCommand { get; }

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

        private async Task OnAddRecipeAsync()
        {
            _logger.LogInformation("AddRecipeCommand triggered");
            try
            {
                await _navigationService.NavigateToAsync(nameof(Views.AddRecipePage));
                _logger.LogDebug("Navigation to AddRecipePage successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation to AddRecipePage failed");
            }
        }

        private async Task OnUpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("UpdateRecipeCommand called with null recipe");
                return;
            }

            _logger.LogInformation("Update recipe requested: {Title}", recipe.Title);

            var parameters = new Dictionary<string, object>
            {
                { "Recipe", recipe }
            };

            try
            {
                await _navigationService.NavigateToAsync(nameof(Views.UpdateRecipePage), parameters);
                _logger.LogDebug("Navigation to UpdateRecipePage successful for {Title}", recipe.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation to UpdateRecipePage failed for {Title}", recipe.Title);
            }
        }

        private async void OnAddToFavorites(Recipe recipe)
        {
            if (recipe == null)
            {
                _logger.LogWarning("AddToFavoritesCommand called with null recipe");
                return;
            }

            if (_repository.AddToFavorites(recipe))
            {
                _logger.LogInformation("Recipe added to favorites: {Title}", recipe.Title);
                recipe.IsFavorite = true;
                try
                {
                    await _dialogService.ShowAlertAsync("Added to Favorites", $"{recipe.Title} was added to your favorites.", "OK");
                    _logger.LogDebug("Alert shown for adding to favorites: {Title}", recipe.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to show alert for adding recipe to favorites: {Title}", recipe.Title);
                }
            }
            else
            {
                _logger.LogInformation("Recipe already in favorites: {Title}", recipe.Title);
                try
                {
                    await _dialogService.ShowAlertAsync("Already a Favorite", $"{recipe.Title} is already in your favorites.", "OK");
                    _logger.LogDebug("Alert shown for recipe already in favorites: {Title}", recipe.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to show alert for already-favorite recipe: {Title}", recipe.Title);
                }
            }
        }

        private async void OnNavigateToFavorites()
        {
            _logger.LogInformation("NavigateToFavoritesCommand triggered");
            try
            {
                await _navigationService.NavigateToAsync(nameof(Views.FavoriteRecipesPage));
                _logger.LogDebug("Navigation to FavoriteRecipesPage successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Navigation to FavoriteRecipesPage failed");
            }
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            _logger.LogInformation("Theme toggled. Dark mode: {IsDarkMode}", _isDarkMode);

            // Update the theme in App.Current.Resources
            App.Current.Resources.MergedDictionaries.Clear();
            if (_isDarkMode)
            {
                App.Current.Resources.MergedDictionaries.Add(new DarkTheme());
            }
            else
            {
                App.Current.Resources.MergedDictionaries.Add(new LightTheme());
            }

            OnPropertyChanged(nameof(ThemeButtonText));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}