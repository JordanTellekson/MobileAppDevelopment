using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public class NavigationService : INavigationService
    {
        public Task NavigateToAsync(string route)
        {
            return Shell.Current.GoToAsync(route);
        }

        public Task NavigateToAsync(string route, Dictionary<string, object> parameters)
        {
            return Shell.Current.GoToAsync(route, parameters);
        }

        public Task GoBackAsync()
        {
            return Shell.Current.GoToAsync("..");
        }
    }
}
