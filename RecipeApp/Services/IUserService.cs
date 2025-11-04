using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public interface IUserService
    {
        public Guid CurrentUserId { get; set; }
        string CurrentToken { get; set; }
        string CurrentUsername { get; set; }
        string CurrentRole { get; set; }
        bool IsAuthenticated { get; }
        string PreferredTheme { get; set; }

        event Action<bool> AuthenticationStateChanged;

        void ApplyTheme(string theme);
        Task<bool> UpdateThemeAsync(string newTheme);
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password, string role = "User");
        void Logout();
    }
}