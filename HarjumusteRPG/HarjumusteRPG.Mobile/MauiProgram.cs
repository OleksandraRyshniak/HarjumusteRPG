using HarjumusteRPG.Mobile.Services;
using HarjumusteRPG.Mobile.ViewModels;
using HarjumusteRPG.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace HarjumusteRPG.Mobile
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
                    fonts.AddFont("PressStart2P-Regular.ttf", "PressStart");
                    fonts.AddFont("PixelifySans-Regular.ttf", "Pixelify");
                });

#if WINDOWS
            // Remove the default Windows border, focus underline and hover color of text fields:
            // the pixel box around the field is drawn by the app itself.
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("PixelEntry", (handler, view) =>
            {
                var box = handler.PlatformView;
                var clear = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);

                box.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                box.Resources["TextControlBorderThemeThickness"] = new Microsoft.UI.Xaml.Thickness(0);
                box.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
                box.Resources["TextControlBorderBrush"] = clear;
                box.Resources["TextControlBorderBrushPointerOver"] = clear;
                box.Resources["TextControlBorderBrushFocused"] = clear;
                box.Resources["TextControlBackground"] = clear;
                box.Resources["TextControlBackgroundPointerOver"] = clear;
                box.Resources["TextControlBackgroundFocused"] = clear;
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Android emulator reaches the PC through 10.0.2.2; Windows uses localhost.
            builder.Services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android
                    ? "http://10.0.2.2:5086/"
                    : "https://localhost:7008/"),
                Timeout = TimeSpan.FromSeconds(15)
            });

            builder.Services.AddSingleton<IAuthService, AuthService>();

            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}