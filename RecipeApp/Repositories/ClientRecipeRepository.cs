using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using RecipeApp.Services;

namespace RecipeApp.Repositories
{
    public class ClientRecipeRepository : IRecipeRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;

        public ObservableCollection<Recipe> Recipes { get; } = new();
        public ObservableCollection<Recipe> Favorites { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public ClientRecipeRepository(HttpClient httpClient, IUserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;

#if ANDROID
            _httpClient.BaseAddress ??= new Uri("https://10.0.2.2:7246/");
#else
            _httpClient.BaseAddress ??= new Uri("https://localhost:7246/");
#endif
        }

        // ---------------------------
        // Initialization
        // ---------------------------
        public async Task InitializeAsync()
        {
            var recipes = await SafeGetAsync<List<Recipe>>("api/recipes") ?? new List<Recipe>();
            var categories = await SafeGetAsync<List<Category>>("api/categories") ?? new List<Category>();

            Recipes.Clear();
            foreach (var r in recipes)
            {
                if (r.CategoryId.HasValue && r.Category == null)
                    r.Category = categories.FirstOrDefault(c => c.Id == r.CategoryId.Value);

                Recipes.Add(r);
            }

            Categories.Clear();
            foreach (var c in categories)
                Categories.Add(c);

            Favorites.Clear();
            foreach (var f in Recipes.Where(r => r.IsFavorite))
                Favorites.Add(f);
        }

        // ---------------------------
        // Recipes CRUD
        // ---------------------------
        public async Task AddRecipeAsync(Recipe recipe)
        {
            if (await SendAuthorizedAsync(HttpMethod.Post, "api/recipes", recipe))
            {
                Recipes.Add(recipe);
                if (recipe.IsFavorite)
                    Favorites.Add(recipe);
            }
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            if (await SendAuthorizedAsync(HttpMethod.Put, $"api/recipes/{recipe.Id}", recipe))
            {
                var existing = Recipes.FirstOrDefault(r => r.Id == recipe.Id);
                if (existing != null)
                {
                    int index = Recipes.IndexOf(existing);
                    Recipes[index] = recipe;

                    if (recipe.IsFavorite)
                    {
                        if (!Favorites.Contains(recipe))
                            Favorites.Add(recipe);
                    }
                    else
                    {
                        Favorites.Remove(recipe);
                    }
                }
            }
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            if (await SendAuthorizedAsync(HttpMethod.Delete, $"api/recipes/{id}"))
            {
                var existing = Recipes.FirstOrDefault(r => r.Id == id);
                if (existing != null)
                {
                    Recipes.Remove(existing);
                    Favorites.Remove(existing);
                }
            }
        }

        public Task<Recipe?> GetRecipeByIdAsync(Guid id) => SafeGetAsync<Recipe>($"api/recipes/{id}");

        // ---------------------------
        // Favorites
        // ---------------------------
        public async Task<bool> AddToFavoritesAsync(Recipe recipe)
        {
            var success = await SendAuthorizedAsync(HttpMethod.Post, $"api/recipes/{recipe.Id}/favorite");
            if (success)
            {
                recipe.IsFavorite = true;
                if (!Favorites.Contains(recipe))
                    Favorites.Add(recipe);
            }
            return success;
        }

        public async Task<bool> RemoveFromFavoritesAsync(Recipe recipe)
        {
            var success = await SendAuthorizedAsync(HttpMethod.Delete, $"api/recipes/{recipe.Id}/favorite");
            if (success)
            {
                recipe.IsFavorite = false;
                Favorites.Remove(recipe);
            }
            return success;
        }

        // ---------------------------
        // Categories CRUD
        // ---------------------------
        public async Task AddCategoryAsync(Category category)
        {
            if (await SendAuthorizedAsync(HttpMethod.Post, "api/categories", category))
                Categories.Add(category);
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            if (await SendAuthorizedAsync(HttpMethod.Put, $"api/categories/{category.Id}", category))
            {
                var existing = Categories.FirstOrDefault(c => c.Id == category.Id);
                if (existing != null)
                {
                    int index = Categories.IndexOf(existing);
                    Categories[index] = category;
                }
            }
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            if (await SendAuthorizedAsync(HttpMethod.Delete, $"api/categories/{id}"))
            {
                var existing = Categories.FirstOrDefault(c => c.Id == id);
                if (existing != null)
                    Categories.Remove(existing);

                foreach (var r in Recipes.Where(r => r.CategoryId == id))
                {
                    r.Category = null;
                    r.CategoryId = null;
                }
            }
        }

        public Task<Category?> GetCategoryByIdAsync(Guid id) => SafeGetAsync<Category>($"api/categories/{id}");

        // ---------------------------
        // Helper Methods
        // ---------------------------
        private async Task<T?> SafeGetAsync<T>(string url)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(url);
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Sends a request without requiring authentication.
        /// </summary>
        private async Task<bool> SendAsync(HttpMethod method, string url, object? data = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url);
                if (data != null)
                    request.Content = JsonContent.Create(data);

                var response = await _httpClient.SendAsync(request);
                await LogResponse(response);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in SendAsync: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Sends a request that requires authentication.
        /// </summary>
        private async Task<bool> SendAuthorizedAsync(HttpMethod method, string url, object? data = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url);

                // Attach Bearer token if available
                if (!string.IsNullOrWhiteSpace(_userService.CurrentToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userService.CurrentToken);
                }

                if (data != null)
                    request.Content = JsonContent.Create(data);

                var response = await _httpClient.SendAsync(request);
                await LogResponse(response);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in SendAuthorizedAsync: {ex}");
                return false;
            }
        }

        private async Task LogResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
                Console.WriteLine($"Response body: {errorContent}");
            }
        }
    }
}