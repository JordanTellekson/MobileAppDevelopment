using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Resources.Styles;
using RecipeApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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

        public ObservableCollection<Recipe> Recipes { get; }

        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IAsyncRelayCommand AddRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> UpdateRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> ToggleFavoriteCommand { get; }
        public IRelayCommand NavigateToFavoritesCommand { get; }
        public IRelayCommand ToggleThemeCommand { get; }

        public string CurrentUser => _userService.CurrentUser;

        private bool _isDarkMode = false;
        public string ThemeButtonText => _isDarkMode ? "Light Mode" : "Dark Mode";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
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

            Recipes = _recipeService.Recipes;

            // Subscribe to collection changes
            Recipes.CollectionChanged += Recipes_CollectionChanged;

            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            AddRecipeCommand = new AsyncRelayCommand(OnAddRecipeAsync);
            UpdateRecipeCommand = new AsyncRelayCommand<Recipe>(OnUpdateRecipeAsync);
            ToggleFavoriteCommand = new AsyncRelayCommand<Recipe>(OnToggleFavoriteAsync);
            NavigateToFavoritesCommand = new RelayCommand(OnNavigateToFavorites);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
        }

        private void Recipes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Notify UI when the collection changes
            OnPropertyChanged(nameof(Recipes));
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            IsLoading = true;
            try
            {
                await _recipeService.InitializeAsync();
                _logger.LogInformation("Recipes initialized. Count: {Count}", Recipes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize recipes");
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
                bool added = await _recipeService.AddToFavoritesAsync(recipe);
                if (added)
                    recipe.IsFavorite = true;
                else
                    await _dialogService.ShowAlertAsync("Already a Favorite", $"{recipe.Title} is already in your favorites.", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding recipe to favorites: {Title}", recipe.Title);
                await _dialogService.ShowAlertAsync("Error", $"Failed to add {recipe.Title} to favorites.", "OK");
            }

            OnPropertyChanged(nameof(Recipes));
        }

        private async Task OnRecipeTappedAsync(Recipe recipe)
        {
            if (recipe == null) return;
            var parameters = new Dictionary<string, object> { { "RecipeId", recipe.Id.ToString() } };
            try { await _navigationService.NavigateToAsync(nameof(Views.RecipeDetailPage), parameters); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation failed for recipe: {Title}", recipe.Title); }
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
                {
                    bool removed = await _recipeService.RemoveFromFavoritesAsync(recipe);
                    if (removed) recipe.IsFavorite = false;
                }
                else
                {
                    bool added = await _recipeService.AddToFavoritesAsync(recipe);
                    if (added) recipe.IsFavorite = true;
                }

                OnPropertyChanged(nameof(Recipes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite for {Title}", recipe.Title);
            }
        }

        private async void OnNavigateToFavorites()
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}