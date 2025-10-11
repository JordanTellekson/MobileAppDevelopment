using AndroidX.Lifecycle;
using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class AddRecipePage : ContentPage
{
    private AddRecipeViewModel ViewModel => BindingContext as AddRecipeViewModel;

    public AddRecipePage(AddRecipeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel != null)
        {
            // Ensure recipes are loaded from JSON
            await ViewModel.InitializeAsync();
        }
    }
}