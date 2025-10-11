using RecipeApp.Shared.Models;
using RecipeApp.ViewModels;
using Microsoft.Maui.Controls;

namespace RecipeApp.Views;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class UpdateRecipePage : ContentPage
{
    private readonly UpdateRecipeViewModel _viewModel;

    public UpdateRecipePage(UpdateRecipeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    private Recipe _recipe;
    public Recipe Recipe
    {
        get => _recipe;
        set
        {
            _recipe = value;
            if (_recipe != null)
            {
                _viewModel.Recipe = _recipe;
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
        }
    }
}