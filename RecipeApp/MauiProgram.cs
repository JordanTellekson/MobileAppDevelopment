using Microsoft.Extensions.Logging;
using RecipeApp.Repositories;
using RecipeApp.Services;
using RecipeApp.Shared.Services;
using RecipeApp.ViewModels;
using RecipeApp.Views;
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
                    fonts.AddFont("fa-solid-900.otf", "FASolid");
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
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });
#endif

            // ---------------------------
            // UserService as singleton with logging
            // ---------------------------
            builder.Services.AddSingleton<IUserService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<UserService>>();

#if DEBUG
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                var client = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
#else
    var client = new HttpClient { BaseAddress = new Uri(apiBase) };
#endif

                return new UserService(client, logger);
            });

            // ---------------------------
            // Other services
            // ---------------------------
            builder.Services.AddSingleton<IRecipeService, RecipeService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // ---------------------------
            // ViewModels
            // ---------------------------
            builder.Services.AddTransient<RecipeListViewModel>();
            builder.Services.AddTransient<AddRecipeViewModel>();
            builder.Services.AddTransient<RecipeDetailViewModel>();
            builder.Services.AddTransient<UpdateRecipeViewModel>();
            builder.Services.AddTransient<FavoriteRecipesViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<LoginViewModel>();

            // ---------------------------
            // Pages
            // ---------------------------
            builder.Services.AddTransient<RecipeListPage>();
            builder.Services.AddTransient<AddRecipePage>();
            builder.Services.AddTransient<RecipeDetailPage>();
            builder.Services.AddTransient<UpdateRecipePage>();
            builder.Services.AddTransient<FavoriteRecipesPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();

            return builder.Build();
        }
    }
}