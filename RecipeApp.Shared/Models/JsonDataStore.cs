using System.Collections.Generic;

namespace RecipeApp.Shared.Models
{
    public class JsonDataStore
    {
        public List<Recipe> Recipes { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}