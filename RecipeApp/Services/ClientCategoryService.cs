using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RecipeApp.Services
{
    public class ClientCategoryService : ICategoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public ClientCategoryService(HttpClient httpClient, IUserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;
        }

        private void AddAuthHeader()
        {
            var token = _userService.CurrentToken;
            if (!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            else
                _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            AddAuthHeader();
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<Category>>("api/Categories");
            return result ?? new List<Category>();
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            AddAuthHeader();
            return await _httpClient.GetFromJsonAsync<Category>($"api/Categories/{id}");
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            AddAuthHeader();
            var response = await _httpClient.PostAsJsonAsync("api/Categories", category);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/Categories/{category.Id}", category);

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"UpdateCategoryAsync failed: {response.StatusCode}, {content}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateCategoryAsync exception: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Categories/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DeleteCategoryAsync failed: {response.StatusCode}, {content}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteCategoryAsync exception: {ex}");
                return false;
            }
        }
    }
}