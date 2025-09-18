using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    public class AddRecipeViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        public AddRecipeViewModel(
            IRecipeRepository repository,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService)
        {
            _repository = repository;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;

            SaveRecipeCommand = new AsyncRelayCommand(OnSaveRecipeAsync);
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

        public IAsyncRelayCommand SaveRecipeCommand { get; }

        private async Task OnSaveRecipeAsync()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await _dialogService.ShowAlertAsync("Error", "Title is required", "OK");
                return;
            }

            int cookingTime = int.TryParse(CookingTimeMinutes, out var minutes) ? minutes : 0;

            var ingredientList = new List<string>();
            if (!string.IsNullOrWhiteSpace(Ingredients))
            {
                foreach (var ing in Ingredients.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(ing))
                        ingredientList.Add(ing.Trim());
                }
            }

            var newRecipe = new Recipe
            {
                Title = Title,
                Description = Description,
                ImageUrl = ImageUrl,
                CookingTimeMinutes = cookingTime,
                Ingredients = ingredientList,
                Instructions = Instructions,
                Author = _userService.CurrentUser
            };

            await _repository.AddRecipeAsync(newRecipe);

            // No reload needed; ObservableCollection updates the UI automatically
            await _navigationService.GoBackAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}