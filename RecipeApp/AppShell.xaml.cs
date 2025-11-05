using RecipeApp.Views;

namespace RecipeApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registering routing
            Routing.RegisterRoute(nameof(RecipeListPage), typeof(RecipeListPage));
            Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
            Routing.RegisterRoute(nameof(AddRecipePage), typeof(AddRecipePage));
            Routing.RegisterRoute(nameof(CategoryPage), typeof(CategoryPage));
            Routing.RegisterRoute(nameof(UpdateRecipePage), typeof(UpdateRecipePage));
            Routing.RegisterRoute(nameof(FavoriteRecipesPage), typeof(FavoriteRecipesPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        }
    }
}
