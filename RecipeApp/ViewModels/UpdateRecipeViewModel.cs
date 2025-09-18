using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(Recipe), "Recipe")]
    public class UpdateRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        public UpdateRecipeViewModel(
            IRecipeRepository repository,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;

            SaveCommand = new AsyncRelayCommand(OnSaveAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
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
                    Title = _recipe.Title;
                    Description = _recipe.Description;
                    ImageUrl = _recipe.ImageUrl;
                    CookingTimeMinutes = _recipe.CookingTimeMinutes.ToString();
                    Ingredients = string.Join(", ", _recipe.Ingredients ?? new List<string>());
                    Instructions = _recipe.Instructions;
                }
            }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get => _imageUrl;
            set { _imageUrl = value; OnPropertyChanged(); }
        }

        private string _cookingTimeMinutes;
        public string CookingTimeMinutes
        {
            get => _cookingTimeMinutes;
            set { _cookingTimeMinutes = value; OnPropertyChanged(); }
        }

        private string _ingredients;
        public string Ingredients
        {
            get => _ingredients;
            set { _ingredients = value; OnPropertyChanged(); }
        }

        private string _instructions;
        public string Instructions
        {
            get => _instructions;
            set { _instructions = value; OnPropertyChanged(); }
        }

        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }

        private async Task OnSaveAsync()
        {
            if (Recipe == null)
            {
                await _dialogService.ShowAlertAsync("Error", "No recipe loaded to update", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(Title))
            {
                await _dialogService.ShowAlertAsync("Error", "Title is required", "OK");
                return;
            }

            // Update recipe properties
            Recipe.Title = Title;
            Recipe.Description = Description;
            Recipe.ImageUrl = ImageUrl;
            Recipe.CookingTimeMinutes = int.TryParse(CookingTimeMinutes, out var minutes) ? minutes : 0;
            Recipe.Ingredients = Ingredients?.Split(',')
                                             .Where(i => !string.IsNullOrWhiteSpace(i))
                                             .Select(i => i.Trim())
                                             .ToList()
                                 ?? new List<string>();
            Recipe.Instructions = Instructions;

            // Save via repository (ObservableCollection ensures UI updates automatically)
            await _repository.UpdateRecipeAsync(Recipe);

            await _navigationService.GoBackAsync();
        }

        private async Task OnCancelAsync()
        {
            await _navigationService.GoBackAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}