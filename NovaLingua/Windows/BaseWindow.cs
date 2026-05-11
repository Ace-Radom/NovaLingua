using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace NovaLingua.Windows;

public class BaseWindow : FluentWindow
{
    protected BaseWindow()
    {
        SnapsToDevicePixels = true;
        ExtendsContentIntoTitleBar = true;
        WindowBackdropType = WindowBackdropType.Mica;
        DpiChanged += BaseWindow_DpiChanged;
        return;
    }

    private void BaseWindow_DpiChanged(object sender, DpiChangedEventArgs e) => VisualTreeHelper.SetRootDpi(this, e.NewDpi);
}
