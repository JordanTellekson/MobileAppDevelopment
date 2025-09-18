using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class AddRecipePage : ContentPage
{
	public AddRecipePage(AddRecipeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}