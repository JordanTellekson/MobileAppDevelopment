using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Services;
using RecipeApp.Repositories;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(Recipe), "Recipe")]
    public class UpdateRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<UpdateRecipeViewModel> _logger;

        public UpdateRecipeViewModel(
            IRecipeService recipeService,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<UpdateRecipeViewModel> logger)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            Categories = new ObservableCollection<Category>();
            FilteredCategories = new ObservableCollection<Category>();

            SaveRecipeCommand = new AsyncRelayCommand(OnSaveAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
            SelectCategoryCommand = new RelayCommand<Category>(OnSelectCategory);
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing UpdateRecipeViewModel...");

            await _recipeService.InitializeAsync();

            Categories.Clear();
            foreach (var c in _recipeService.Categories)
                Categories.Add(c);

            _logger.LogInformation("Loaded {Count} categories for UpdateRecipePage.", Categories.Count);
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

                    // Set initial category text
                    if (!string.IsNullOrWhiteSpace(_recipe.CategoryName))
                        CategoryText = _recipe.CategoryName;
                }
            }
        }

        // ---------------------------
        // Recipe fields
        // ---------------------------
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

        // ---------------------------
        // Categories for autocomplete
        // ---------------------------
        public ObservableCollection<Category> Categories { get; }
        public ObservableCollection<Category> FilteredCategories { get; }

        private string _categoryText;
        public string CategoryText
        {
            get => _categoryText;
            set
            {
                _categoryText = value;
                OnPropertyChanged();
                UpdateFilteredCategories();
            }
        }

        private bool _isCategorySuggestionsVisible;
        public bool IsCategorySuggestionsVisible
        {
            get => _isCategorySuggestionsVisible;
            set { _isCategorySuggestionsVisible = value; OnPropertyChanged(); }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                if (value != null)
                    CategoryText = value.Name;
            }
        }

        // ---------------------------
        // Commands
        // ---------------------------
        public IAsyncRelayCommand SaveRecipeCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public RelayCommand<Category> SelectCategoryCommand { get; }

        private void OnSelectCategory(Category category)
        {
            if (category == null) return;
            SelectedCategory = category;
            IsCategorySuggestionsVisible = false;
        }

        private void UpdateFilteredCategories()
        {
            FilteredCategories.Clear();

            if (string.IsNullOrWhiteSpace(CategoryText))
            {
                IsCategorySuggestionsVisible = false;
                return;
            }

            var filtered = Categories
                .Where(c => c.Name.Contains(CategoryText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var c in filtered)
                FilteredCategories.Add(c);

            IsCategorySuggestionsVisible = filtered.Any();
        }

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

            Recipe.Title = Title;
            Recipe.Description = Description;
            Recipe.ImageUrl = ImageUrl;
            Recipe.CookingTimeMinutes = int.TryParse(CookingTimeMinutes, out var minutes) ? minutes : 0;
            Recipe.Ingredients = Ingredients?.Split(',')
                                             .Select(i => i.Trim())
                                             .Where(i => !string.IsNullOrWhiteSpace(i))
                                             .ToList() ?? new List<string>();
            Recipe.Instructions = Instructions;

            // Set selected category
            Recipe.CategoryId = SelectedCategory?.Id ?? Guid.Empty;
            Recipe.CategoryName = SelectedCategory?.Name ?? CategoryText;

            _logger.LogInformation("Updating recipe: {Title}", Recipe.Title);

            try
            {
                await _recipeService.UpdateRecipeAsync(Recipe);
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