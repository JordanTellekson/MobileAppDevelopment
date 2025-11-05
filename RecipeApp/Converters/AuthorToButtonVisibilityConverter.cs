using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using RecipeApp.ViewModels;

namespace RecipeApp.Converters
{
    public class AuthorToButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string author) 
                return false;

            // Parameter should be the page so we can get the ViewModel
            if (parameter is ContentPage page && page.BindingContext is RecipeListViewModel vm)
            {
                // Return true if current user is the author OR is an admin
                return author == vm.CurrentUser || vm.CurrentRole == "Admin";
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
