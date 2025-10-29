using RecipeApp.Shared.DTOs;
using RecipeApp.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace RecipeApp.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;

        public string CurrentToken { get; set; } = string.Empty;
        public string CurrentUsername { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(CurrentToken);

        // 🔹 New event implementation
        public event Action<bool> AuthenticationStateChanged;

        public UserService(HttpClient httpClient, ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

                    _logger.LogInformation("Login successful for user: {Username}", username);

                    // 🔹 Notify subscribers
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

                    _logger.LogInformation("Registration successful for user: {Username}", username);

                    // 🔹 Notify subscribers
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

        public void Logout()
        {
            _logger.LogInformation("Logging out user: {Username}", CurrentUsername);

            CurrentToken = string.Empty;
            CurrentUsername = string.Empty;
            CurrentRole = string.Empty;

            // 🔹 Notify subscribers
            AuthenticationStateChanged?.Invoke(false);
        }
    }
}