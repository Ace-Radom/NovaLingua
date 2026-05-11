using System.Diagnostics;
using System.Reflection;
using NovaLingua.Extensions;

namespace NovaLingua.Windows
{
    public partial class MainWindow : BaseWindow
    {
        public MainWindow(string projectFilePath)
        {
            InitializeComponent();

            UpdateTitle();
        }

        private void UpdateTitle(string LoadedProjName = "")
        {
            string newTitle = string.IsNullOrWhiteSpace(LoadedProjName)
                ? "NovaLingua"
                : $"{LoadedProjName} - NovaLingua";
#if DEBUG
            newTitle += Debugger.IsAttached ? " [DEBUGGER ATTACHED]" : "[DEBUG]";
#else
            var version = Assembly.GetEntryAssembly()?.GetName().Version;
            if (version is not null)
            {
                if (version.IsBeta()) newTitle += " [BETA]";
                else newTitle += $" {version.Major}.{version.Minor}.{version.Build}";
            }
#endif
            _titleTextBlock.Text = newTitle;
            return;
        }
    }
}