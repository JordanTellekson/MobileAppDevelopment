using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using RecipeApp.Services;
using RecipeApp.ViewModels;

namespace RecipeApp.Converters
{
    public class AuthorToButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string author) return false;

            if (parameter is ContentPage page && page.BindingContext is RecipeListViewModel vm)
            {
                return author == vm.CurrentUser;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}