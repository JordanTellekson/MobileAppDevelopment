using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Services;
using RecipeApp.Shared.Services;

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

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Category> FilteredCategories { get; } = new();

        public IAsyncRelayCommand SaveRecipeCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public RelayCommand<Category> SelectCategoryCommand { get; }

        private Recipe _recipe;
        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                SetProperty(ref _recipe, value);

                if (_recipe != null)
                {
                    Title = _recipe.Title;
                    Description = _recipe.Description;
                    ImageUrl = _recipe.ImageUrl;
                    CookingTimeMinutes = _recipe.CookingTimeMinutes.ToString();
                    Ingredients = string.Join(", ", _recipe.Ingredients ?? new List<string>());
                    Instructions = _recipe.Instructions;
                    SelectedCategory = _recipe.Category;
                    CategoryText = SelectedCategory?.Name ?? string.Empty;
                }
            }
        }

        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private string _description;
        public string Description { get => _description; set => SetProperty(ref _description, value); }

        private string _imageUrl;
        public string ImageUrl { get => _imageUrl; set => SetProperty(ref _imageUrl, value); }

        private string _cookingTimeMinutes;
        public string CookingTimeMinutes { get => _cookingTimeMinutes; set => SetProperty(ref _cookingTimeMinutes, value); }

        private string _ingredients;
        public string Ingredients { get => _ingredients; set => SetProperty(ref _ingredients, value); }

        private string _instructions;
        public string Instructions { get => _instructions; set => SetProperty(ref _instructions, value); }

        private string _categoryText;
        public string CategoryText
        {
            get => _categoryText;
            set
            {
                SetProperty(ref _categoryText, value);
                UpdateFilteredCategories();
            }
        }

        private bool _isCategorySuggestionsVisible;
        public bool IsCategorySuggestionsVisible
        {
            get => _isCategorySuggestionsVisible;
            set => SetProperty(ref _isCategorySuggestionsVisible, value);
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                if (value != null)
                    CategoryText = value.Name;
            }
        }

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

            SaveRecipeCommand = new AsyncRelayCommand(OnSaveAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
            SelectCategoryCommand = new RelayCommand<Category>(OnSelectCategory);
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _recipeService.InitializeAsync();

                Categories.Clear();
                foreach (var c in _recipeService.Categories)
                    Categories.Add(c);

                _logger.LogInformation("Loaded {Count} categories.", Categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize UpdateRecipeViewModel.");
                await _dialogService.ShowAlertAsync("Error", "Failed to load categories.", "OK");
            }
        }

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
                await _dialogService.ShowAlertAsync("Error", "No recipe loaded.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                await _dialogService.ShowAlertAsync("Error", "Title is required.", "OK");
                return;
            }

            // Handle category
            if (!string.IsNullOrWhiteSpace(CategoryText))
            {
                SelectedCategory ??= Categories.FirstOrDefault(c => c.Name.Equals(CategoryText.Trim(), StringComparison.OrdinalIgnoreCase));

                if (SelectedCategory == null)
                {
                    SelectedCategory = new Category { Name = CategoryText.Trim() };
                    await _recipeService.AddCategoryAsync(SelectedCategory);
                    Categories.Add(SelectedCategory);
                }
            }

            // Update recipe properties
            Recipe.Title = Title;
            Recipe.Description = Description;
            Recipe.ImageUrl = ImageUrl;
            Recipe.CookingTimeMinutes = int.TryParse(CookingTimeMinutes, out var minutes) ? minutes : 0;
            Recipe.Ingredients = Ingredients?.Split(',').Select(i => i.Trim()).Where(i => !string.IsNullOrWhiteSpace(i)).ToList()
                                 ?? new List<string>();
            Recipe.Instructions = Instructions;
            Recipe.Category = SelectedCategory;
            Recipe.CategoryId = SelectedCategory?.Id;

            try
            {
                await _recipeService.UpdateRecipeAsync(Recipe);
                await _navigationService.GoBackAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe: {Title}", Recipe.Title);
                await _dialogService.ShowAlertAsync("Error", "Failed to update recipe.", "OK");
            }
        }

        private async Task OnCancelAsync()
        {
            await _navigationService.GoBackAsync();
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