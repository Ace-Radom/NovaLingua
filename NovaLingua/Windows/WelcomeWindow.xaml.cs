using Microsoft.Win32;
using System.Windows;

namespace NovaLingua.Windows;

public partial class WelcomeWindow : BaseWindow
{
    public WelcomeWindow()
    {
        InitializeComponent();
    }

    private void LoadProjectButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "NovaLingua Data File|*.nld",
            RestoreDirectory = true,
            Title = "Select Project File"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            var projectFilePath = openFileDialog.FileName;

            var mainWindow = new MainWindow(projectFilePath);
            mainWindow.Show();
            Close();
        }
        else
        {

        }
    }
}
