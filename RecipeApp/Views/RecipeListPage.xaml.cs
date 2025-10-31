using Microsoft.Maui.Controls;
using RecipeApp.Services; // Add this to access DialogService
using RecipeApp.Shared.Models;
using RecipeApp.ViewModels;
using System.Collections.Specialized;

namespace RecipeApp.Views;

public partial class RecipeListPage : ContentPage
{
    private RecipeListViewModel ViewModel => BindingContext as RecipeListViewModel;
    private readonly IDialogService _dialogService;

    public RecipeListPage(RecipeListViewModel viewModel, IDialogService dialogService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _dialogService = dialogService;

        // Initial toolbar setup
        UpdateToolbar();

        // Refresh toolbar whenever ToolbarItems changes
        viewModel.ToolbarItems.CollectionChanged += ToolbarItems_CollectionChanged;

        // Subscribe to authentication changes
        viewModel.UserService.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private void ToolbarItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateToolbar();
    }

    private void OnAuthenticationStateChanged(bool isAuthenticated)
    {
        // Refresh toolbar on main thread
        MainThread.BeginInvokeOnMainThread(UpdateToolbar);
    }

    private void UpdateToolbar()
    {
        ToolbarItems.Clear();
        if (ViewModel?.ToolbarItems == null) return;

        foreach (var item in ViewModel.ToolbarItems)
            ToolbarItems.Add(item);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!ViewModel.IsLoading)
        {
            await ViewModel.SoftRefreshRecipesAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (ViewModel != null)
        {
            ViewModel.ToolbarItems.CollectionChanged -= ToolbarItems_CollectionChanged;
            ViewModel.UserService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }
    }

    private async void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
    {
        if (sender is SwipeView swipeView && swipeView.BindingContext is Recipe recipe)
        {
            if (ViewModel?.UserService?.IsAuthenticated != true)
            {
                await _dialogService.ShowAlertAsync(
                    "Login Required",
                    "Please sign in to favorite recipes.",
                    "OK"
                );

                // Instantly close the swipe to prevent it from staying open
                swipeView.Close();
                return;
            }

            try
            {
                await ViewModel.AddToFavoritesAsync(recipe);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add recipe to favorites: {ex.Message}");
            }

            swipeView.Close();
        }
    }
}