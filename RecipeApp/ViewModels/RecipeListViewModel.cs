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
using Microsoft.Maui.Controls;

namespace RecipeApp.ViewModels
{
    public class RecipeListViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<RecipeListViewModel> _logger;

        public IUserService UserService => _userService;

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();

        // Toolbar items for dynamic show/hide
        public ObservableCollection<ToolbarItem> ToolbarItems { get; } = new();

        // Commands
        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IAsyncRelayCommand AddRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> UpdateRecipeCommand { get; }
        public IAsyncRelayCommand<Recipe> ToggleFavoriteCommand { get; }
        public IAsyncRelayCommand NavigateToFavoritesCommand { get; }
        public IRelayCommand ToggleThemeCommand { get; }
        public IAsyncRelayCommand LogoutCommand { get; }
        public IAsyncRelayCommand NavigateToRegisterCommand { get; }
        public IAsyncRelayCommand NavigateToLoginCommand { get; }
        public IAsyncRelayCommand NavigateToCategoriesCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

        private bool _isDarkMode;
        public string CurrentUser => _userService.CurrentUsername;
        public string CurrentRole => _userService.CurrentRole;
        public string ThemeButtonText => _isDarkMode ? "Light Mode" : "Dark Mode";

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isAuthenticated;

        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => SetProperty(ref _isAuthenticated, value);
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private bool _initialized = false;

        public RecipeListViewModel(
            IRecipeService recipeService,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            IFavoriteService favoriteService,
            ILogger<RecipeListViewModel> logger)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _favoriteService = favoriteService;
            _logger = logger;

            _isAuthenticated = _userService.IsAuthenticated;

            // Subscribe to authentication state changes
            _userService.AuthenticationStateChanged += OnAuthenticationStateChanged;

            // Recipe commands
            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            AddRecipeCommand = new AsyncRelayCommand(OnAddRecipeAsync);
            UpdateRecipeCommand = new AsyncRelayCommand<Recipe>(OnUpdateRecipeAsync);
            ToggleFavoriteCommand = new AsyncRelayCommand<Recipe>(OnToggleFavoriteAsync);
            NavigateToFavoritesCommand = new AsyncRelayCommand(OnNavigateToFavoritesAsync);
            NavigateToCategoriesCommand = new AsyncRelayCommand(OnNavigateToCategoriesAsync);

            // UI commands
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);
            NavigateToRegisterCommand = new AsyncRelayCommand(NavigateToRegisterAsync);
            NavigateToLoginCommand = new AsyncRelayCommand(NavigateToLoginAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshRecipesAsync);

            // Initial toolbar setup using current authentication state
            BuildToolbar(_userService.IsAuthenticated);
        }

        // 🔹 Updated signature for event handler
        private void OnAuthenticationStateChanged(bool isAuthenticated)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsAuthenticated = isAuthenticated;
                BuildToolbar(isAuthenticated);

                if (isAuthenticated)
                {
                    // 🔹 Apply the user’s saved preference
                    ApplyTheme(_userService.PreferredTheme ?? "Light");
                }
                else
                {
                    // 🔹 Reset when logged out
                    ResetTheme();
                }
            });
        }

        public async Task RefreshRecipesAsync()
        {
            if (_recipeService == null) return;

            try
            {
                IsRefreshing = true;
                // Reload recipes
                await _recipeService.InitializeAsync();

                Recipes.Clear();
                foreach (var r in _recipeService.Recipes)
                    Recipes.Add(r);

                Favorites.Clear();
                foreach (var r in _recipeService.Favorites)
                    Favorites.Add(r);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to refresh recipes.", "OK");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task SoftRefreshRecipesAsync()
        {
            if (_recipeService == null) return;

            try
            {
                // Get fresh data from the backend without resetting everything
                var oldRecipes = new List<Recipe>(Recipes);
                var oldFavorites = new List<Recipe>(Favorites);

                await _recipeService.InitializeAsync();

                // Update Recipes collection efficiently
                UpdateCollection(Recipes, _recipeService.Recipes);

                // Update Favorites collection efficiently
                UpdateCollection(Favorites, _recipeService.Favorites);
            }
            catch (Exception)
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to refresh recipes.", "OK");
            }
        }

        private void UpdateCollection(ObservableCollection<Recipe> target, IEnumerable<Recipe> source)
        {
            var targetSet = new HashSet<Guid>(target.Select(r => r.Id));

            // Add new recipes
            foreach (var recipe in source)
            {
                if (!targetSet.Contains(recipe.Id))
                    target.Add(recipe);
            }

            // Remove deleted recipes
            for (int i = target.Count - 1; i >= 0; i--)
            {
                if (!source.Any(r => r.Id == target[i].Id))
                    target.RemoveAt(i);
            }

            // Update modified recipes (title, favorites, etc.)
            foreach (var recipe in source)
            {
                var existing = target.FirstOrDefault(r => r.Id == recipe.Id);
                if (existing != null)
                {
                    existing.Title = recipe.Title;
                    existing.ImageUrl = recipe.ImageUrl;
                    existing.IsFavorite = recipe.IsFavorite;
                }
            }
        }

        #region Toolbar Management
        public void BuildToolbar(bool isAuthenticated)
        {
            ToolbarItems.Clear();

            if (isAuthenticated)
            {
                if (_userService.CurrentRole == "Admin")
                {
                    ToolbarItems.Add(new ToolbarItem("Categories", null, async () => await NavigateToCategoriesCommand.ExecuteAsync(null)));
                }
                ToolbarItems.Add(new ToolbarItem("Add", null, async () => await AddRecipeCommand.ExecuteAsync(null)));
                ToolbarItems.Add(new ToolbarItem("Favorites", null, async () => await NavigateToFavoritesCommand.ExecuteAsync(null)));
                ToolbarItems.Add(new ToolbarItem("Theme", null, () => ToggleThemeCommand.Execute(null)));
                ToolbarItems.Add(new ToolbarItem("Logout", null, async () => await LogoutCommand.ExecuteAsync(null)));
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem("Register", null, async () => await NavigateToRegisterCommand.ExecuteAsync(null)));
                ToolbarItems.Add(new ToolbarItem("Login", null, async () => await NavigateToLoginCommand.ExecuteAsync(null)));
            }
        }
        #endregion

        public async Task LogoutAsync()
        {
            _userService.Logout(); // 🔹 Fires AuthenticationStateChanged(false)
            await _navigationService.NavigateToAsync(nameof(Views.RecipeListPage));
        }

        private Task NavigateToRegisterAsync() => _navigationService.NavigateToAsync(nameof(Views.RegisterPage));
        private Task NavigateToLoginAsync() => _navigationService.NavigateToAsync(nameof(Views.LoginPage));

        #region Recipes & Favorites
        public async Task InitializeAsync(bool forceReload = false)
        {
            if (_initialized && !forceReload)
                return;

            _initialized = true;
            IsLoading = true;

            try
            {
                // Load all recipes and categories from the repository
                await _recipeService.InitializeAsync();

                // Clear existing collections
                Recipes.Clear();
                Favorites.Clear();

                // Populate recipes
                foreach (var r in _recipeService.Recipes)
                {
                    // Reset IsFavorite for all recipes
                    r.IsFavorite = false;
                    Recipes.Add(r);
                }

                // Only populate favorites if user is logged in
                if (_userService.IsAuthenticated)
                {
                    var userId = _userService.CurrentUserId;
                    var backendFavorites = await _favoriteService.GetUserFavoritesAsync(userId);

                    foreach (var recipe in backendFavorites)
                    {
                        Favorites.Add(recipe);

                        // Mark the corresponding recipe in the main collection as favorite
                        var match = Recipes.FirstOrDefault(r => r.Id == recipe.Id);
                        if (match != null)
                            match.IsFavorite = true;
                    }
                }

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

            if (!_userService.IsAuthenticated)
            {
                await _dialogService.ShowAlertAsync(
                    "Login Required",
                    "You must be logged in to favorite recipes.",
                    "OK"
                );
                _logger.LogInformation("Favorite action blocked: user not authenticated.");
                return;
            }

            try
            {
                var userId = _userService.CurrentUserId;

                if (recipe.IsFavorite)
                {
                    var removed = await _favoriteService.RemoveFavoriteAsync(userId, recipe.Id);
                    if (removed)
                    {
                        recipe.IsFavorite = false;
                        _logger.LogInformation("Removed recipe {Title} from favorites", recipe.Title);
                    }
                }
                else
                {
                    var added = await _favoriteService.AddFavoriteAsync(userId, recipe.Id);
                    if (added)
                    {
                        recipe.IsFavorite = true;
                        _logger.LogInformation("Added recipe {Title} to favorites", recipe.Title);
                        await _dialogService.ShowAlertAsync("Added to Favorites", $"{recipe.Title} was added to your favorites!", "OK");
                    }
                }

                var backendFavorites = await _favoriteService.GetUserFavoritesAsync(userId);

                Favorites.Clear();
                foreach (var r in backendFavorites)
                    Favorites.Add(r);

                await SoftRefreshRecipesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle favorite for {Title}", recipe.Title);
                await _dialogService.ShowAlertAsync("Error", $"Failed to toggle favorite for {recipe.Title}.", "OK");
            }
        }

        private async Task OnNavigateToFavoritesAsync()
        {
            try { await _navigationService.NavigateToAsync(nameof(Views.FavoriteRecipesPage)); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation to FavoriteRecipesPage failed"); }
        }

        private async Task OnNavigateToCategoriesAsync()
        {
            try { await _navigationService.NavigateToAsync(nameof(Views.CategoryPage)); }
            catch (Exception ex) { _logger.LogError(ex, "Navigation to CategoryPage failed"); }
        }
        #endregion

        #region Theme
        private void ApplyTheme(string theme)
        {
            App.Current.Resources.MergedDictionaries.Clear();

            if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
            {
                App.Current.Resources.MergedDictionaries.Add(new DarkTheme());
                _isDarkMode = true;
            }
            else
            {
                App.Current.Resources.MergedDictionaries.Add(new LightTheme());
                _isDarkMode = false;
            }

            OnPropertyChanged(nameof(ThemeButtonText));
        }

        // Called when user taps the theme toggle button
        private async void ToggleTheme()
        {
            if (!_userService.IsAuthenticated)
            {
                await _dialogService.ShowAlertAsync("Not Logged In", "You must be logged in to change themes.", "OK");
                return;
            }

            var newTheme = _isDarkMode ? "Light" : "Dark";
            _logger.LogInformation("User toggling theme to {Theme}", newTheme);

            // Update locally right away
            ApplyTheme(newTheme);

            // Persist to backend
            var success = await _userService.UpdateThemeAsync(newTheme);
            if (!success)
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to save theme preference.", "OK");
            }
        }

        public void ResetTheme()
        {
            _isDarkMode = false;
            App.Current.Resources.MergedDictionaries.Clear();
            App.Current.Resources.MergedDictionaries.Add(new LightTheme());
            OnPropertyChanged(nameof(ThemeButtonText));
        }
        #endregion

        #region Cleanup & INotifyPropertyChanged
        public void Dispose()
        {
            _userService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

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