using Microsoft.Extensions.Logging;
using RecipeApp.Repositories;
using RecipeApp.Services;
using RecipeApp.Shared.Services;
using RecipeApp.ViewModels;
using RecipeApp.Views;
using static RecipeApp.Services.IUserService;
using System.Net.Http;

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
            // API Base URL
            // ---------------------------
#if ANDROID
            string apiBase = "https://10.0.2.2:7223/"; // Android emulator loopback
#else
            string apiBase = "https://localhost:7223/"; // Windows/macOS
#endif

            // ---------------------------
            // Repositories with HttpClient
            // ---------------------------
            builder.Services.AddHttpClient<IRecipeRepository, ApiRecipeRepository>(client =>
            {
                client.BaseAddress = new Uri(apiBase);
            })
#if DEBUG
            // Development: ignore SSL errors (self-signed certs)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });
#endif

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