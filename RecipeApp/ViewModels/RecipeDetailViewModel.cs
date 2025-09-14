using Microsoft.Maui.Controls;
using MvvmHelpers;
using RecipeApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecipeApp.ViewModels
{
    [QueryProperty(nameof(Recipe), "Recipe")]
    public class RecipeDetailViewModel : BaseViewModel
    {
        private Recipe _recipe;
        public Recipe Recipe
        {
            get => _recipe;
            set => SetProperty(ref _recipe, value);
        }
    }
}
