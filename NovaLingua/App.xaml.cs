using System.Windows;
using NovaLingua.Windows;

namespace NovaLingua
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var welcomeWindow = new WelcomeWindow();
            welcomeWindow.Show();
        }
    }

}
