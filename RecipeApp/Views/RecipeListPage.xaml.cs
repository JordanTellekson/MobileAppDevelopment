using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class RecipeListPage : ContentPage
{
	public RecipeListPage(RecipeListViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}