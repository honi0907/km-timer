using System.Runtime.InteropServices;
using KmTimer.Helpers;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Windows.Graphics;

namespace KmTimer;

public sealed partial class OutputWindow : Window
{
    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    public OutputWindow()
    {
        InitializeComponent();
        ExtendsContentIntoTitleBar = false;
        Surface.DataContext = App.MainViewModel;
    }

    public void ShowOnDisplay()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var appWindow = OutputWindowLayout.GetAppWindow(this);
        var secondary = MonitorHelper.GetSecondaryMonitorBounds(useFullMonitorBounds: true);

        if (secondary is not null)
        {
            // フルスクリーン: WinUI MoveAndResize / OverlappedPresenter 枠消しは使わない
            OutputWindowLayout.PrepareWin32FullscreenChrome(this, appWindow);
            appWindow.Show();
            var bounds = MonitorHelper.ExpandBounds(secondary.Value, overscanPixels: 2);
            MonitorHelper.ApplyWin32BorderlessAndBounds(hwnd, bounds, popupStyle: true);
            Title = "KM_Timer Output";
        }
        else
        {
            var scale = GetDpiForWindow(hwnd) / 96.0;
            OutputWindowLayout.PlaceWindowedNearPrimary(appWindow, 1280, 720, scale);
            appWindow.Show();
            Activate();
            Title = "KM_Timer Output（ディスプレイ1台）";
        }
    }
}
