using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace RecipeApp.Popups
{
    public class AlertPage : ContentPage
    {
        public AlertPage(string title, string message)
        {
            // Fully transparent overlay
            BackgroundColor = Colors.Transparent;
            Padding = 0;
            NavigationPage.SetHasNavigationBar(this, false);

            var accentColor = Color.FromArgb("#B22222");
            var pageBgColor = Color.FromArgb("#FFF5E6");

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
                        // Added a custom shadow for better visibility
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
                                    HorizontalOptions = LayoutOptions.Center
                                },
                                new Button
                                {
                                    Text = "OK",
                                    BackgroundColor = pageBgColor,
                                    TextColor = accentColor,
                                    Command = new Command(async () =>
                                    {
                                        await Navigation.PopModalAsync();
                                    }),
                                    HorizontalOptions = LayoutOptions.Center
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}