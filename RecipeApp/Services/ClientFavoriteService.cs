using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public class ClientFavoriteService : IFavoriteService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public ClientFavoriteService(HttpClient httpClient, IUserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;
        }

        private void ApplyAuthHeader()
        {
            if (!string.IsNullOrWhiteSpace(_userService.CurrentToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _userService.CurrentToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<bool> AddFavoriteAsync(Guid userId, Guid recipeId)
        {
            ApplyAuthHeader();
            var response = await _httpClient.PostAsync($"api/favorites/{recipeId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveFavoriteAsync(Guid userId, Guid recipeId)
        {
            ApplyAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/favorites/{recipeId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<Recipe>> GetUserFavoritesAsync(Guid userId)
        {
            ApplyAuthHeader();
            var favorites = await _httpClient.GetFromJsonAsync<IEnumerable<Recipe>>("api/favorites");
            return favorites ?? new List<Recipe>();
        }
    }
}