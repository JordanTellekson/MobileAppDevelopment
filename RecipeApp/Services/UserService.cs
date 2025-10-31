using RecipeApp.Shared.DTOs;
using RecipeApp.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Maui.Controls;
using RecipeApp.Resources.Styles;

namespace RecipeApp.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;

        public string CurrentToken { get; set; } = string.Empty;
        public string CurrentUsername { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public string PreferredTheme { get; set; } = "Light";
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(CurrentToken);

        public event Action<bool> AuthenticationStateChanged;

        public UserService(HttpClient httpClient, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private void ApplyAuthHeader()
        {
            if (!string.IsNullOrWhiteSpace(CurrentToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        // 🔹 Helper to switch themes at runtime
        public void ApplyTheme(string theme)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            if (theme == "Dark")
                Application.Current.Resources.MergedDictionaries.Add(new DarkTheme());
            else
                Application.Current.Resources.MergedDictionaries.Add(new LightTheme());
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Username}", username);

                var request = new AuthRequest { Username = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login failed for user {Username}, status code: {StatusCode}", username, response.StatusCode);
                    return false;
                }

                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    CurrentToken = authResponse.Token;
                    CurrentUsername = authResponse.Username;
                    CurrentRole = authResponse.Role;
                    PreferredTheme = authResponse.PreferredTheme;

                    ApplyAuthHeader();
                    ApplyTheme(PreferredTheme);

                    _logger.LogInformation("Login successful for user: {Username}", username);
                    AuthenticationStateChanged?.Invoke(true);
                    return true;
                }

                _logger.LogWarning("Login failed for user {Username}, empty response", username);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during login for user: {Username}", username);
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string role = "User")
        {
            try
            {
                _logger.LogInformation("Attempting registration for user: {Username}", username);

                var request = new RegisterRequest { Username = username, Password = password, Role = role };
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Registration failed for user {Username}, status code: {StatusCode}", username, response.StatusCode);
                    return false;
                }

                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    CurrentToken = authResponse.Token;
                    CurrentUsername = authResponse.Username;
                    CurrentRole = authResponse.Role;
                    PreferredTheme = authResponse.PreferredTheme;

                    ApplyAuthHeader();
                    ApplyTheme(PreferredTheme);

                    _logger.LogInformation("Registration successful for user: {Username}", username);
                    AuthenticationStateChanged?.Invoke(true);
                    return true;
                }

                _logger.LogWarning("Registration failed for user {Username}, empty response", username);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during registration for user: {Username}", username);
                return false;
            }
        }

        public async Task<bool> UpdateThemeAsync(string newTheme)
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentUsername))
                    return false;

                var dto = new ThemeUpdate
                {
                    Username = CurrentUsername,
                    Theme = newTheme
                };

                var response = await _httpClient.PutAsJsonAsync("api/auth/theme", dto);

                if (response.IsSuccessStatusCode)
                {
                    PreferredTheme = newTheme;
                    ApplyTheme(newTheme);
                    return true;
                }

                _logger.LogWarning("Failed to update theme for {Username}, status: {Status}", CurrentUsername, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating theme for {Username}", CurrentUsername);
                return false;
            }
        }

        public void Logout()
        {
            _logger.LogInformation("Logging out user: {Username}", CurrentUsername);

            CurrentToken = string.Empty;
            CurrentUsername = string.Empty;
            CurrentRole = string.Empty;
            PreferredTheme = "Light";

            _httpClient.DefaultRequestHeaders.Authorization = null;

            ApplyTheme("Light");
            AuthenticationStateChanged?.Invoke(false);
        }
    }
}