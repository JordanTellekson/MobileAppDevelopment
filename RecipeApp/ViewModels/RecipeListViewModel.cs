using CommunityToolkit.Mvvm.Input;
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

        public RecipeListViewModel(
            IRecipeRepository repository,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;

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
            if (recipe == null) return;

            var parameters = new Dictionary<string, object>
            {
                { "RecipeId", recipe.Id.ToString() }
            };

            await _navigationService.NavigateToAsync(nameof(Views.RecipeDetailPage), parameters);
        }

        private async Task OnAddRecipeAsync()
        {
            await _navigationService.NavigateToAsync(nameof(Views.AddRecipePage));
        }

        private async Task OnUpdateRecipeAsync(Recipe recipe)
        {
            if (recipe == null) return;

            var parameters = new Dictionary<string, object>
            {
                { "Recipe", recipe }
            };

            await _navigationService.NavigateToAsync(nameof(Views.UpdateRecipePage), parameters);
        }

        private async void OnAddToFavorites(Recipe recipe)
        {
            if (recipe == null) return;

            if (_repository.AddToFavorites(recipe))
            {
                await _dialogService.ShowAlertAsync("Added to Favorites", $"{recipe.Title} was added to your favorites.", "OK");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Already a Favorite", $"{recipe.Title} is already in your favorites.", "OK");
            }
        }

        private async void OnNavigateToFavorites()
        {
            // Navigate to FavoritesPage
            await _navigationService.NavigateToAsync(nameof(Views.FavoriteRecipesPage));
        }

        private void ToggleTheme()
        {
            _isDarkMode = !_isDarkMode;

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

            // Notify button text to update
            OnPropertyChanged(nameof(ThemeButtonText));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}