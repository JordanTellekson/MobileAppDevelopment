using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Services;
using RecipeApp.Shared.Services;
using RecipeApp.Resources.Styles;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Linq;

namespace RecipeApp.ViewModels
{
    public class CategoriesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;
        private readonly ILogger<CategoriesViewModel> _logger;

        public CategoriesViewModel(
            ICategoryService categoryService,
            IDialogService dialogService,
            INavigationService navigationService,
            IUserService userService,
            ILogger<CategoriesViewModel> logger)
        {
            _categoryService = categoryService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userService = userService;
            _logger = logger;

            Categories = new ObservableCollection<Category>();

            LoadCategoriesCommand = new AsyncRelayCommand(() => InitializeAsync());
            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            EditCategoryCommand = new AsyncRelayCommand<Category>(UpdateCategoryAsync);
            DeleteCategoryCommand = new AsyncRelayCommand<Category>(DeleteCategoryAsync);
        }

        public ObservableCollection<Category> Categories { get; }

        // Commands
        public IAsyncRelayCommand LoadCategoriesCommand { get; }
        public IAsyncRelayCommand AddCategoryCommand { get; }
        public IAsyncRelayCommand<Category> EditCategoryCommand { get; }
        public IAsyncRelayCommand<Category> DeleteCategoryCommand { get; }

        private string _newCategoryName;
        public string NewCategoryName
        {
            get => _newCategoryName;
            set => SetProperty(ref _newCategoryName, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _initialized = false;

        public async Task InitializeAsync(bool forceReload = false)
        {
            if (_initialized && !forceReload) return;

            _initialized = true;
            IsLoading = true;

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var c in categories)
                    Categories.Add(c);

                _logger.LogInformation("Loaded {Count} categories.", Categories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories.");
                await _dialogService.ShowAlertAsync("Error", "Failed to load categories.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;

            var category = new Category { Name = NewCategoryName.Trim() };
            var success = await _categoryService.AddCategoryAsync(category);

            if (success)
            {
                await InitializeAsync(forceReload: true); // refresh list
                NewCategoryName = string.Empty;
                await _dialogService.ShowAlertAsync("Added", $"Category '{category.Name}' added.", "OK");
                _logger.LogInformation("Added category: {Name}", category.Name);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to add category.", "OK");
            }
        }

        private async Task UpdateCategoryAsync(Category category)
        {
            if (category == null) return;

            var success = await _categoryService.UpdateCategoryAsync(category);
            if (success)
            {
                await InitializeAsync(forceReload: true); // refresh list
                await _dialogService.ShowAlertAsync("Updated", $"Category '{category.Name}' updated.", "OK");
                _logger.LogInformation("Updated category: {Name}", category.Name);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to update category.", "OK");
            }
        }

        private async Task DeleteCategoryAsync(Category category)
        {
            if (category == null) return;

            bool confirm = await _dialogService.ShowConfirmAsync(
                "Delete Category",
                $"Are you sure you want to delete '{category.Name}'?",
                "Accept",
                "Delete"
            );

            if (!confirm) return;

            var success = await _categoryService.DeleteCategoryAsync(category.Id);
            if (success)
            {
                await InitializeAsync(forceReload: true); // refresh list
                await _dialogService.ShowAlertAsync("Deleted", $"Category '{category.Name}' deleted.", "OK");
                _logger.LogInformation("Deleted category: {Name}", category.Name);
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to delete category.", "OK");
            }
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

        public void Dispose() { }
    }
}