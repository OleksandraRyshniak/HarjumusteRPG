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
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton(new HttpClient
            {
                
                BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android
        ? "http://localhost:5086/"
        : "https://localhost:7008/")
            });
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<MainPage>();
            return builder.Build();
        }
    }
}
