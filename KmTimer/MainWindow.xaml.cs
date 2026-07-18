using System.Runtime.InteropServices;
using KmTimer.ViewModels;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace KmTimer;

public sealed partial class MainWindow : Window
{
    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppTitleBar.Title = App.MainViewModel.AppVersionText;
        Title = App.MainViewModel.AppVersionText;

        try
        {
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico"));
        }
        catch
        {
            // Icon is optional at design/dev time.
        }

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var scale = GetDpiForWindow(hwnd) / 96.0;
        AppWindow.Resize(new SizeInt32((int)(1160 * scale), (int)(800 * scale)));

        RootFrame.Navigate(typeof(MainPage));

        Closed += MainWindow_Closed;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        App.Shutdown();
    }
}
