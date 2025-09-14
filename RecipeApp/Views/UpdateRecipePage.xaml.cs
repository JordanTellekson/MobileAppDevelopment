using RecipeApp.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Views;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class UpdateRecipePage : ContentPage
{
    private Recipe _recipe;

    public Recipe Recipe
    {
        get => _recipe;
        set
        {
            _recipe = value;

            // Set the BindingContext once Recipe is passed in
            BindingContext = new UpdateRecipeViewModel
            {
                Recipe = _recipe
            };
        }
    }

    public UpdateRecipePage()
    {
        InitializeComponent();
    }
}