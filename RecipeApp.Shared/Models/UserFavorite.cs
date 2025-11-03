using System;

namespace RecipeApp.Shared.Models
{
    public class UserFavorite
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid RecipeId { get; set; }

        public User? User { get; set; }
        public Recipe? Recipe { get; set; }
    }
}