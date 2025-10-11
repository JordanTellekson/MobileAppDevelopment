using RecipeApp.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RecipeApp.Repositories
{
    public interface ICategoryRepository
    {
        Task InitializeAsync();
        ObservableCollection<Category> Categories { get; }
        Task AddCategoryAsync(Category category);
        Task UpdateCategoryAsync(Category category);
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task DeleteCategoryAsync(Guid id);
    }
}