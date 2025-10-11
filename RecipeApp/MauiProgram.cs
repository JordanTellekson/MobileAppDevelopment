using Microsoft.Extensions.Logging;
using RecipeApp.Repositories;
using RecipeApp.Services;
using RecipeApp.ViewModels;
using RecipeApp.Views;
using static RecipeApp.Services.IUserService;

namespace RecipeApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

            // ---------------------------
            // Repositories
            // ---------------------------
            builder.Services.AddSingleton<IRecipeRepository, RecipeRepository>();
            builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();

            // ---------------------------
            // Services
            // ---------------------------
            builder.Services.AddSingleton<IRecipeService, RecipeService>();
            builder.Services.AddTransient<IDialogService, DialogService>();
            builder.Services.AddTransient<INavigationService, NavigationService>();
            builder.Services.AddTransient<IUserService, UserService>();

            // ---------------------------
            // ViewModels
            // ---------------------------
            builder.Services.AddTransient<RecipeListViewModel>();
            builder.Services.AddTransient<AddRecipeViewModel>();
            builder.Services.AddTransient<RecipeDetailViewModel>();
            builder.Services.AddTransient<UpdateRecipeViewModel>();
            builder.Services.AddTransient<FavoriteRecipesViewModel>();

            // ---------------------------
            // Pages
            // ---------------------------
            builder.Services.AddTransient<RecipeListPage>();
            builder.Services.AddTransient<AddRecipePage>();
            builder.Services.AddTransient<RecipeDetailPage>();
            builder.Services.AddTransient<UpdateRecipePage>();
            builder.Services.AddTransient<FavoriteRecipesPage>();

            return builder.Build();
        }
    }
}