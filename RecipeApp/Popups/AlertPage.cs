using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Threading.Tasks;

namespace RecipeApp.Popups
{
    public class AlertPage : ContentPage
    {
        public AlertPage(string title, string message)
        {
            // Transparent overlay
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

        public static async Task<bool> ShowConfirmationAsync(INavigation navigation, string title, string message)
        {
            var page = new ContentPage
            {
                BackgroundColor = Colors.Transparent,
                Padding = 0
            };

            NavigationPage.SetHasNavigationBar(page, false);

            var accentColor = Color.FromArgb("#B22222");
            var pageBgColor = Color.FromArgb("#FFF5E6");
            var tcs = new TaskCompletionSource<bool>();

            page.Content = new Grid
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
                                    Spacing = 20,
                                    Children =
                                    {
                                        new Button
                                        {
                                            Text = "Cancel",
                                            BackgroundColor = pageBgColor,
                                            TextColor = accentColor,
                                            Command = new Command(async () =>
                                            {
                                                tcs.TrySetResult(false);
                                                await navigation.PopModalAsync();
                                            }),
                                            WidthRequest = 100
                                        },
                                        new Button
                                        {
                                            Text = "Delete",
                                            BackgroundColor = Colors.White,
                                            TextColor = Colors.Red,
                                            Command = new Command(async () =>
                                            {
                                                tcs.TrySetResult(true);
                                                await navigation.PopModalAsync();
                                            }),
                                            WidthRequest = 100
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await navigation.PushModalAsync(page);
            return await tcs.Task;
        }
    }
}