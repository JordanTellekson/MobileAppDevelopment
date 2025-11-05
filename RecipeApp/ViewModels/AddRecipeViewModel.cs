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
using RecipeApp.Shared.Services;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public class AddRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeService _recipeService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<AddRecipeViewModel> _logger;

        public AddRecipeViewModel(
            IRecipeService recipeService,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<AddRecipeViewModel> logger)
        {
            _recipeService = recipeService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            Categories = new ObservableCollection<Category>();
            FilteredCategories = new ObservableCollection<Category>();

            SaveRecipeCommand = new AsyncRelayCommand(OnSaveRecipeAsync);
            SelectCategoryCommand = new RelayCommand<Category>(OnSelectCategory);
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing AddRecipeViewModel...");
            await _recipeService.InitializeAsync();

            Categories.Clear();
            foreach (var c in _recipeService.Categories)
                Categories.Add(c);

            _logger.LogInformation("Loaded {Count} categories for AddRecipePage.", Categories.Count);
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

        private async Task OnSaveRecipeAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await _dialogService.ShowAlertAsync("Error", "Title is required", "OK");
                return;
            }

            // Parse ingredients
            var ingredientList = string.IsNullOrWhiteSpace(Ingredients)
                ? new List<string>()
                : Ingredients.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(i => i.Trim())
                             .ToList();

            // Parse cooking time
            int.TryParse(CookingTimeMinutes, out var cookingTime);

            // Ensure category exists
            if (SelectedCategory == null && !string.IsNullOrWhiteSpace(CategoryText))
            {
                SelectedCategory = new Category { Name = CategoryText.Trim() };
                await _recipeService.AddCategoryAsync(SelectedCategory);
                Categories.Add(SelectedCategory);
            }

            var newRecipe = new Recipe
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl,
                CookingTimeMinutes = cookingTime,
                Ingredients = ingredientList,
                Instructions = Instructions,
                Author = _userService.CurrentUsername,
                Category = SelectedCategory,
                CategoryId = SelectedCategory?.Id ?? Guid.Empty
            };

            try
            {
                await _recipeService.AddRecipeAsync(newRecipe);

                // Clear fields after save
                Title = Description = ImageUrl = CookingTimeMinutes = Ingredients = Instructions = CategoryText = string.Empty;
                SelectedCategory = null;
                IsCategorySuggestionsVisible = false;

                await _navigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving recipe: {Title}", newRecipe.Title);
                await _dialogService.ShowAlertAsync("Error", "Failed to save recipe", "OK");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}