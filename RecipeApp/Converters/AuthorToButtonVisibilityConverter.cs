using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace RecipeApp.Converters
{
    public class AuthorToButtonVisibilityConverter : IValueConverter
    {
        // Converts a recipe's author to a boolean for button visibility
        // Returns true if the recipe's author matches the current user
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string author = value as string;
            return author == ViewModels.RecipeListViewModel.CurrentAuthor;
        }

        // ConvertBack not implemented because one-way binding only
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}