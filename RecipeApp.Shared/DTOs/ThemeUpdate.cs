using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Shared.DTOs
{
    public class ThemeUpdate
    {
        public string Username { get; set; } = string.Empty;
        public string Theme { get; set; } = "Light";
    }
}