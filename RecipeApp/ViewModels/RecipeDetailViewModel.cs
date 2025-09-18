using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RecipeApp.Models;
using RecipeApp.Services;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(RecipeId), "RecipeId")]
    public class RecipeDetailViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public RecipeDetailViewModel(
            IRecipeRepository repository,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _repository = repository;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

        private Recipe _recipe;
        public Recipe Recipe
        {
            get => _recipe;
            set
            {
                _recipe = value;
                OnPropertyChanged();
            }
        }

        private string _recipeId;
        public string RecipeId
        {
            get => _recipeId;
            set
            {
                _recipeId = value;
                _ = LoadRecipeAsync(); // Fire-and-forget loading
            }
        }

        private async Task LoadRecipeAsync()
        {
            if (Guid.TryParse(RecipeId, out var id))
            {
                Recipe = await _repository.GetRecipeByIdAsync(id);

                if (Recipe == null)
                {
                    await _dialogService.ShowAlertAsync("Error", "Recipe not found", "OK");
                    await _navigationService.GoBackAsync();
                }
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "Invalid Recipe ID", "OK");
                await _navigationService.GoBackAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}