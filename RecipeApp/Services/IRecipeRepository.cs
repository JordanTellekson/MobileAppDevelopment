using RecipeApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public interface IRecipeRepository
    {
        ObservableCollection<Recipe> Recipes { get; }
        Task AddRecipeAsync(Recipe recipe);
        Task UpdateRecipeAsync(Recipe recipe);
        Task<Recipe?> GetRecipeByIdAsync(Guid id);
    }
}
