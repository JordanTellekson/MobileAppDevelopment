using Microsoft.Extensions.Logging;
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
#endif

            builder.Services.AddSingleton<IRecipeRepository, RecipeRepository>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<IUserService, UserService>();


            builder.Services.AddTransient<RecipeListViewModel>();
            builder.Services.AddTransient<AddRecipeViewModel>();
            builder.Services.AddTransient<RecipeDetailViewModel>();
            builder.Services.AddTransient<UpdateRecipeViewModel>();

            builder.Services.AddTransient<RecipeListPage>();
            builder.Services.AddTransient<AddRecipePage>();
            builder.Services.AddTransient<RecipeDetailPage>();
            builder.Services.AddTransient<UpdateRecipePage>();

            return builder.Build();
        }
    }
}
