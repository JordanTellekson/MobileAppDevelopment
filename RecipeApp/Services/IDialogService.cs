using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Services
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string buttonText);
    }
}
