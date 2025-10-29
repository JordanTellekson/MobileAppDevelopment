using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using RecipeApp.Services;
using System.Threading.Tasks;

namespace RecipeApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<LoginViewModel> _logger;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string username = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool isBusy = false;

        public LoginViewModel(
            IUserService userService,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<LoginViewModel> logger)
        {
            _userService = userService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;
        }

        // Automatically generates LoginCommand
        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task LoginAsync()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", Username);

                var success = await _userService.LoginAsync(Username, Password);

                if (!success)
                {
                    ErrorMessage = "Invalid username or password.";
                    _logger.LogWarning("Login failed for user: {Username}", Username);
                    return;
                }

                _logger.LogInformation("Login successful for user: {Username}", Username);

                try
                {
                    await _navigationService.NavigateToAsync(nameof(Views.RecipeListPage));
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Navigation to RecipeListPage failed after login");
                    await _dialogService.ShowAlertAsync("Navigation Error", "Unable to navigate to recipe list.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "An unexpected error occurred. Please try again.";
                _logger.LogError(ex, "Login error for user: {Username}", Username);
            }
            finally
            {
                IsBusy = false;
                // Notify that CanExecute might have changed
                LoginCommand.NotifyCanExecuteChanged();
            }
        }

        // Determines if the login button should be enabled
        private bool CanLogin() =>
            !IsBusy &&
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password);
    }
}