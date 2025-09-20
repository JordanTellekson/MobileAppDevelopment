using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public class DialogService : IDialogService
    {
        public async Task ShowAlertAsync(string title, string message, string buttonText)
        {
            await Application.Current.MainPage.Navigation.PushModalAsync(
                new Popups.AlertPage(title, message)
            );
        }
    }
}