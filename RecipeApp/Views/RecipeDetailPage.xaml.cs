using RecipeApp.Models;
using RecipeApp.ViewModels;
using Microsoft.Maui.Controls;

namespace RecipeApp.Views;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}