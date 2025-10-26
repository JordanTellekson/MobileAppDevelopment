using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace RecipeApp.Popups
{
    public class ConfirmDeletePage : ContentPage
    {
        private readonly Func<bool, Task> _onResult;

        public ConfirmDeletePage(string title, string message, Func<bool, Task> onResult)
        {
            _onResult = onResult;

            BackgroundColor = Colors.Transparent;
            Padding = 0;
            NavigationPage.SetHasNavigationBar(this, false);

            var accentColor = Color.FromArgb("#B22222"); // firebrick
            var pageBgColor = Color.FromArgb("#FFF5E6"); // soft cream

            Content = new Grid
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Frame
                    {
                        BackgroundColor = accentColor,
                        CornerRadius = 15,
                        Padding = new Thickness(20),
                        HasShadow = true,
                        Shadow = new Shadow
                        {
                            Brush = Colors.Black,
                            Offset = new Point(0, 5),
                            Opacity = 0.5f,
                            Radius = 15
                        },
                        Content = new StackLayout
                        {
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center,
                            Spacing = 15,
                            Children =
                            {
                                new Label
                                {
                                    Text = title,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = pageBgColor,
                                    HorizontalOptions = LayoutOptions.Center
                                },
                                new Label
                                {
                                    Text = message,
                                    TextColor = pageBgColor,
                                    HorizontalOptions = LayoutOptions.Center,
                                    HorizontalTextAlignment = TextAlignment.Center
                                },
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    HorizontalOptions = LayoutOptions.Center,
                                    Spacing = 15,
                                    Children =
                                    {
                                        new Button
                                        {
                                            Text = "Delete",
                                            BackgroundColor = pageBgColor,
                                            TextColor = accentColor,
                                            Command = new Command(async () => await _onResult(true))
                                        },
                                        new Button
                                        {
                                            Text = "Cancel",
                                            BackgroundColor = pageBgColor,
                                            TextColor = accentColor,
                                            Command = new Command(async () => await _onResult(false))
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}