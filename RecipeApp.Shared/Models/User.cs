using System;

namespace RecipeApp.Shared.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string? Token { get; set; }
        public string PreferredTheme { get; set; } = "Light";
    }
}