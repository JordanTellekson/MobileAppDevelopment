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

    private async void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        if (sender is SwipeView swipeView && swipeView.BindingContext is Recipe recipe)
        {
            if (BindingContext is RecipeListViewModel vm)
            {
                try
                {
                    // Call the async method safely
                    await vm.AddToFavoritesAsync(recipe);
                }
                catch (Exception ex)
                {
                    // Optional: log or display error
                    Console.WriteLine($"Failed to add recipe to favorites: {ex.Message}");
                }
            }
            swipeView.Close(); // reset swipe visually
        }
    }
}