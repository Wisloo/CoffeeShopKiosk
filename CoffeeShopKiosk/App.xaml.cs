using System.Windows;

namespace CoffeeShopKiosk
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Attach a global handler to capture and log XAML parse errors and other unhandled exceptions during startup
            this.DispatcherUnhandledException += (s, exArgs) =>
            {
                try
                {
                    var path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                    System.IO.File.WriteAllText(path, exArgs.Exception.ToString());
                }
                catch { }

                // Allow Visual Studio/IDE to still break on exceptions during debugging
                // and mark the exception as handled so the app doesn't crash silently when we log it.
                exArgs.Handled = false;
            };

            base.OnStartup(e);
        }
    }
}