using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RecipeApp.Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace RecipeApp.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<RegisterViewModel> _logger;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        [ObservableProperty]
        private bool isRegisterEnabled = false;

        private bool _hasUsernameTyped = false;
        private bool _hasPasswordTyped = false;

        public ICommand RegisterCommand { get; }

        public RegisterViewModel(IUserService userService, INavigationService navigationService,
                                 IDialogService dialogService, ILogger<RegisterViewModel> logger)
        {
            _userService = userService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            RegisterCommand = new Command(async () => await RegisterAsync(), () => IsRegisterEnabled);

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(Username))
                {
                    _hasUsernameTyped = !string.IsNullOrEmpty(Username);
                    Validate();
                }
                else if (e.PropertyName == nameof(Password))
                {
                    _hasPasswordTyped = !string.IsNullOrEmpty(Password);
                    Validate();
                }
                else if (e.PropertyName == nameof(IsRegisterEnabled))
                {
                    ((Command)RegisterCommand).ChangeCanExecute();
                }
            };
        }

        private void Validate()
        {
            string? usernameError = null;
            string? passwordError = null;

            if (_hasUsernameTyped && Username.Length < 3)
                usernameError = "Username must be at least 3 characters.";

            if (_hasPasswordTyped)
            {
                if (Password.Length < 8)
                    passwordError = "Password must be at least 8 characters.";
                else if (!Regex.IsMatch(Password, @"\d"))
                    passwordError = "Password must contain at least one number.";
                else if (!Regex.IsMatch(Password, @"[!@#$%^&*(),.?""{}|<>]"))
                    passwordError = "Password must contain at least one symbol.";
            }

            ErrorMessage = usernameError ?? passwordError ?? string.Empty;

            IsRegisterEnabled = string.IsNullOrEmpty(ErrorMessage)
                                && !string.IsNullOrWhiteSpace(Username)
                                && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task RegisterAsync()
        {
            if (IsBusy || !IsRegisterEnabled) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                _logger.LogInformation("Attempting registration for user: {Username}", Username);
                var success = await _userService.RegisterAsync(Username, Password);

                if (!success)
                {
                    ErrorMessage = "Registration failed. Username may already exist.";
                    _logger.LogWarning("Registration failed for user: {Username}", Username);
                    return;
                }

                // Registration successful
                _logger.LogInformation("Registration successful for user: {Username}", Username);
                ErrorMessage = "Registration successful!";

                // Show the success message for a moment
                await Task.Delay(800);

                try
                {
                    // Navigate to LoginPage after registration
                    await _navigationService.NavigateToAsync(nameof(Views.LoginPage));
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Navigation to LoginPage failed after registration");
                    // Optionally, show a dialog instead of overwriting ErrorMessage
                    await _dialogService.ShowAlertAsync("Navigation Error", "Unable to navigate to login page.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "An unexpected error occurred. Please try again.";
                _logger.LogError(ex, "Registration error for user: {Username}", Username);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}