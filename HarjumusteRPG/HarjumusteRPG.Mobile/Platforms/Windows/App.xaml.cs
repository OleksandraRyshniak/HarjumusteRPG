using Microsoft.UI.Xaml;

namespace HarjumusteRPG.Mobile.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();

            // MAUI Shell on Windows sometimes throws this from its own internal async code
            // when navigations overlap. It is harmless: ignore it instead of crashing.
            this.UnhandledException += (s, e) =>
            {
                if (e.Exception is InvalidOperationException &&
                    e.Exception.Message.Contains("Pending Navigations still processing"))
                {
                    e.Handled = true;
                }
            };
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}