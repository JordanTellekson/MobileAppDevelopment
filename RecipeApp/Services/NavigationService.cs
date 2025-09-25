using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RecipeApp.Services
{
    public class NavigationService : INavigationService
    {
        private readonly ILogger<NavigationService> _logger;

        public NavigationService(ILogger<NavigationService> logger)
        {
            _logger = logger;
        }

        public Task NavigateToAsync(string route)
        {
            _logger.LogInformation("Navigating to route: {Route}", route);
            return Shell.Current.GoToAsync(route);
        }

        public Task NavigateToAsync(string route, Dictionary<string, object> parameters)
        {
            _logger.LogInformation("Navigating to route: {Route} with parameters: {@Parameters}", route, parameters);
            return Shell.Current.GoToAsync(route, parameters);
        }

        public Task GoBackAsync()
        {
            _logger.LogInformation("Navigating back");
            return Shell.Current.GoToAsync("..");
        }
    }
}