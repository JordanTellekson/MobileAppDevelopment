using System.Threading.Tasks;
using RecipeApp.Shared.DTOs;

namespace RecipeApp.Shared.Services
{
    public interface IAuthService
    {
        Task<AuthResponse?> RegisterAsync(RegisterRequest request);
        Task<AuthResponse?> LoginAsync(AuthRequest request);
    }
}