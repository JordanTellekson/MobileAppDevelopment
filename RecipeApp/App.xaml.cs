using RecipeApp.Services;

namespace RecipeApp
{
    public partial class App : Application
    {
        public App(IRecipeService recipeService)
        {
            InitializeComponent();

            MainPage = new AppShell();

            // Fire-and-forget async initialization
            Task.Run(async () =>
            {
                await recipeService.InitializeAsync();
            });
        }
    }
}