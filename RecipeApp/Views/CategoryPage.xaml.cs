using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class CategoryPage : ContentPage
{
    public CategoriesViewModel ViewModel { get; }

    public CategoryPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = ViewModel;

        _ = ViewModel.InitializeAsync();
    }
}