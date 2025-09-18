using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateToAsync(string route, Dictionary<string, object> parameters);
        Task GoBackAsync();
    }
}
