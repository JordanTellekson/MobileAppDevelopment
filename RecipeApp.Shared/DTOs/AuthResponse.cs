namespace RecipeApp.Shared.DTOs
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string PreferredTheme { get; set; } = "Light";
    }
}