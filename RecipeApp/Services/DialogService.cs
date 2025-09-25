using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RecipeApp.Services
{
    public class DialogService : IDialogService
    {
        private readonly ILogger<DialogService> _logger;

        public DialogService(ILogger<DialogService> logger)
        {
            _logger = logger;
        }

        public async Task ShowAlertAsync(string title, string message, string buttonText)
        {
            _logger.LogInformation("Showing alert. Title: {Title}, Message: {Message}, Button: {Button}",
                title, message, buttonText);

            try
            {
                await Application.Current.MainPage.Navigation.PushModalAsync(
                    new Popups.AlertPage(title, message)
                );

                _logger.LogDebug("Alert displayed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to display alert with Title: {Title}", title);
                throw; // rethrow so caller still knows it failed
            }
        }
    }
}