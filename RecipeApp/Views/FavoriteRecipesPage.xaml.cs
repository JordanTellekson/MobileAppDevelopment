using RecipeApp.Shared.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class FavoriteRecipesPage : ContentPage
{
    private readonly FavoriteRecipesViewModel _viewModel;

    public FavoriteRecipesPage(FavoriteRecipesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        if (sender is SwipeView swipeView && swipeView.BindingContext is Recipe recipe)
        {
            if (BindingContext is FavoriteRecipesViewModel vm)
            {
                vm.RemoveFromFavoritesCommand?.Execute(recipe);
            }
            swipeView.Close(); // reset swipe visually
        }
    }
}