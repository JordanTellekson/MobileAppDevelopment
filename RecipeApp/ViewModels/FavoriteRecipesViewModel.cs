using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using RecipeApp.Models;
using RecipeApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeApp.ViewModels
{
    public class FavoriteRecipesViewModel : INotifyPropertyChanged
    {
        private readonly IRecipeRepository _repository;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;

        public FavoriteRecipesViewModel(
            IRecipeRepository repository,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            _repository = repository;
            _navigationService = navigationService;
            _dialogService = dialogService;

            // Bind directly to repository's Favorites collection
            Favorites = _repository.Favorites;

            RecipeTappedCommand = new AsyncRelayCommand<Recipe>(OnRecipeTappedAsync);
            RemoveFromFavoritesCommand = new RelayCommand<Recipe>(OnRemoveFromFavorites);
        }

        public ObservableCollection<Recipe> Favorites { get; }

        public IAsyncRelayCommand<Recipe> RecipeTappedCommand { get; }
        public IRelayCommand<Recipe> RemoveFromFavoritesCommand { get; }

        private async Task OnRecipeTappedAsync(Recipe recipe)
        {
            if (recipe == null) return;

            var parameters = new Dictionary<string, object>
            {
                { "RecipeId", recipe.Id.ToString() }
            };

            await _navigationService.NavigateToAsync(nameof(Views.RecipeDetailPage), parameters);
        }

        private async void OnRemoveFromFavorites(Recipe recipe)
        {
            if (recipe == null) return;

            if (_repository.RemoveFromFavorites(recipe))
            {
                await _dialogService.ShowAlertAsync("Removed", $"{recipe.Title} removed from favorites.", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}