using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using RecipeApp.Services;
using RecipeApp.Resources.Styles;
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
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<RecipeListViewModel> _logger;

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();

        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IAsyncRelayCommand AddRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> UpdateRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> ToggleFavoriteCommand { get; }
        public IAsyncRelayCommand NavigateToFavoritesCommand { get; }
        public IRelayCommand ToggleThemeCommand { get; }

        private bool _isDarkMode;
        public string CurrentUser => _userService.CurrentUser;
        public string ThemeButtonText => _isDarkMode ? "Light Mode" : "Dark Mode";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _initialized = false;

        public RecipeListViewModel(
            IRecipeService recipeService,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<RecipeListViewModel> logger)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            AddRecipeCommand = new AsyncRelayCommand(OnAddRecipeAsync);
            UpdateRecipeCommand = new AsyncRelayCommand<Recipe>(OnUpdateRecipeAsync);
            ToggleFavoriteCommand = new AsyncRelayCommand<Recipe>(OnToggleFavoriteAsync);
            NavigateToFavoritesCommand = new AsyncRelayCommand(OnNavigateToFavoritesAsync);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
        }

        public async Task InitializeAsync(bool forceReload = false)
        {
            if (_initialized && !forceReload)
                return;

            _initialized = true;
            IsLoading = true;

            try
            {
                await _recipeService.InitializeAsync();

                Recipes.Clear();
                foreach (var r in _recipeService.Recipes)
                    Recipes.Add(r);

                Favorites.Clear();
                foreach (var r in _recipeService.Favorites)
                    Favorites.Add(r);

                _logger.LogInformation("Recipes initialized: {Count}", Recipes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize recipes");
                await _dialogService.ShowAlertAsync("Error", "Failed to load recipes.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task AddToFavoritesAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                if (recipe.IsFavorite)
                {
                    await _dialogService.ShowAlertAsync("Already Added",
                        $"{recipe.Title} is already in your favorites.", "OK");
                    _logger.LogInformation("Recipe already in favorites: {Title}", recipe.Title);
                    return;
                }

                await _recipeService.AddToFavoritesAsync(recipe);
                recipe.IsFavorite = true;

                await _dialogService.ShowAlertAsync("Added",
                    $"{recipe.Title} added to favorites!", "OK");
                _logger.LogInformation("Added recipe to favorites: {Title}", recipe.Title);

                // Sync Favorites collection
                Favorites.Clear();
                foreach (var r in _recipeService.Favorites)
                    Favorites.Add(r);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add recipe to favorites: {Title}", recipe.Title);
                await _dialogService.ShowAlertAsync("Error",
                    $"Failed to add {recipe.Title} to favorites.", "OK");
            }
        }

        private async Task OnRecipeTappedAsync(Recipe recipe)
        {
            if (recipe == null) return;

            var parameters = new Dictionary<string, object> { { "RecipeId", recipe.Id.ToString() } };
            try { await _navigationService.NavigateToAsync(nameof(Views.RecipeDetailPage), parameters); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation failed for recipe {Title}", recipe.Title); }
        }

        private async Task OnAddRecipeAsync()
        {
            try { await _navigationService.NavigateToAsync(nameof(Views.AddRecipePage)); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation to AddRecipePage failed"); }
        }

        private async Task OnUpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;

            var parameters = new Dictionary<string, object> { { "Recipe", recipe } };
            try { await _navigationService.NavigateToAsync(nameof(Views.UpdateRecipePage), parameters); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation to UpdateRecipePage failed for {Title}", recipe.Title); }
        }

        private async Task OnToggleFavoriteAsync(Recipe recipe)
        {
            if (recipe == null) return;

            try
            {
                if (recipe.IsFavorite)
                    await _recipeService.RemoveFromFavoritesAsync(recipe);
                else
                    await _recipeService.AddToFavoritesAsync(recipe);

                recipe.IsFavorite = !recipe.IsFavorite;

                // Sync Favorites collection
                Favorites.Clear();
                foreach (var r in _recipeService.Favorites)
                    Favorites.Add(r);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite for {Title}", recipe.Title);
            }
        }

        private async Task OnNavigateToFavoritesAsync()
        {
            try { await _navigationService.NavigateToAsync(nameof(Views.FavoriteRecipesPage)); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation to FavoriteRecipesPage failed"); }
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;
            App.Current.Resources.MergedDictionaries.Clear();
            App.Current.Resources.MergedDictionaries.Add(_isDarkMode ? new DarkTheme() : new LightTheme());
            OnPropertyChanged(nameof(ThemeButtonText));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}