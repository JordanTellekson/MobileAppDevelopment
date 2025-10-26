using RecipeApp.Shared.Models;
using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class RecipeListPage : ContentPage
{
    private RecipeListViewModel ViewModel => BindingContext as RecipeListViewModel;

    public RecipeListPage(RecipeListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (ViewModel != null)
        {
            await ViewModel.InitializeAsync(forceReload: true);
        }
    }

    private async void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        if (sender is SwipeView swipeView && swipeView.BindingContext is Recipe recipe)
        {
            if (ViewModel != null)
            {
                try
                {
                    await ViewModel.AddToFavoritesAsync(recipe);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add recipe to favorites: {ex.Message}");
                }
            }
            swipeView.Close(); // reset swipe visually
        }
    }
}