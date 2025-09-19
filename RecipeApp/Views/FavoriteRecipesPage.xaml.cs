using RecipeApp.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class FavoriteRecipesPage : ContentPage
{
    public FavoriteRecipesPage(FavoriteRecipesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
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