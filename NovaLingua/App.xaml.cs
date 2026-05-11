using System.Windows;
using NovaLingua.Lib;
using NovaLingua.Windows;
using WinFormsApp = System.Windows.Forms.Application;
using WinFormsHighDpiMode = System.Windows.Forms.HighDpiMode;

namespace NovaLingua;

public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        WinFormsApp.SetHighDpiMode(WinFormsHighDpiMode.PerMonitorV2);

        IoCContainer.Initialize(
            new Lib.IoCModule()
        );

        var welcomeWindow = new WelcomeWindow();
        welcomeWindow.Show();
        return;
    }
}
