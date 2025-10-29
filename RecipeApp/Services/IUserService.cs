using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public interface IUserService
    {
        string CurrentToken { get; set; }
        string CurrentUsername { get; set; }
        string CurrentRole { get; set; }
        bool IsAuthenticated { get; }

        event Action<bool> AuthenticationStateChanged;

        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password, string role = "User");
        void Logout();
    }
}