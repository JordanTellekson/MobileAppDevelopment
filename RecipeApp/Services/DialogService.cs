using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowAlertAsync(string title, string message, string buttonText)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, buttonText);
        }
    }
}
