using RecipeApp.Shared.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class RecipeListPage : ContentPage
{
    public RecipeListPage(RecipeListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        if (sender is SwipeView swipeView && swipeView.BindingContext is Recipe recipe)
        {
            if (BindingContext is RecipeListViewModel vm)
            {
                vm.AddToFavoritesCommand?.Execute(recipe);
            }
            swipeView.Close(); // reset swipe visually
        }
    }
}