using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(RecipeId), "RecipeId")]
    public class RecipeDetailViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeService _recipeService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<RecipeDetailViewModel> _logger;

        public RecipeDetailViewModel(
            IRecipeService recipeService,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<RecipeDetailViewModel> logger)
        {
            _recipeService = recipeService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;
        }

        private Recipe _recipe;
        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                SetProperty(ref _recipe, value);
                OnPropertyChanged(nameof(CategoryDisplay));
            }
        }

        private string _recipeId;
        public string RecipeId
        {
            get => _recipeId;
            set
            {
                _recipeId = value;
                _logger.LogInformation("Recipe ID set for detail view: {RecipeId}", _recipeId);
                _ = LoadRecipeAsync();
            }
        }

        public string CategoryDisplay => string.IsNullOrWhiteSpace(Recipe?.Category?.Name)
            ? "Uncategorized"
            : $"Category: {Recipe.Category.Name}";

        private async Task LoadRecipeAsync()
        {
            if (!Guid.TryParse(RecipeId, out var id))
            {
                _logger.LogWarning("Invalid Recipe ID: {RecipeId}", RecipeId);
                await _dialogService.ShowAlertAsync("Error", "Invalid Recipe ID", "OK");
                await _navigationService.GoBackAsync();
                return;
            }

            try
            {
                Recipe = await _recipeService.GetRecipeByIdAsync(id);

                if (Recipe == null)
                {
                    _logger.LogWarning("Recipe not found: {RecipeId}", RecipeId);
                    await _dialogService.ShowAlertAsync("Error", "Recipe not found", "OK");
                    await _navigationService.GoBackAsync();
                }
                else
                {
                    _logger.LogInformation("Loaded recipe: {Title}", Recipe.Title);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipe: {RecipeId}", RecipeId);
                await _dialogService.ShowAlertAsync("Error", "Failed to load recipe", "OK");
                await _navigationService.GoBackAsync();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(backingStore, value)) return false;
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}