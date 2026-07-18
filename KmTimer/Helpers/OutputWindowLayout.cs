using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;
using Windows.Graphics;

namespace KmTimer.Helpers;

internal static class OutputWindowLayout
{
    public static AppWindow GetAppWindow(Window window)
    {
        var hwnd = WindowNative.GetWindowHandle(window);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        return AppWindow.GetFromWindowId(windowId);
    }

    public static void PrepareWin32FullscreenChrome(Window window, AppWindow appWindow)
    {
        try
        {
            ClearWindowDragRegions(window, appWindow);
            CollapseTitleBar(appWindow);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OutputWindowLayout] PrepareWin32FullscreenChrome failed: {ex}");
        }
    }

    public static void ConfigureNormalPresenter(AppWindow appWindow)
    {
        try
        {
            var presenter = OverlappedPresenter.Create();
            presenter.SetBorderAndTitleBar(true, true);
            presenter.IsMaximizable = true;
            presenter.IsMinimizable = true;
            presenter.IsResizable = true;
            appWindow.SetPresenter(presenter);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OutputWindowLayout] ConfigureNormalPresenter failed: {ex}");
        }
    }

    public static void PlaceWindowedNearPrimary(AppWindow appWindow, int widthDip, int heightDip, double scale)
    {
        ConfigureNormalPresenter(appWindow);
        var primary = MonitorHelper.GetPrimaryMonitorBounds(useFullMonitorBounds: false);
        var width = (int)(widthDip * scale);
        var height = (int)(heightDip * scale);
        var left = (primary?.Left ?? 0) + (int)(80 * scale);
        var top = (primary?.Top ?? 0) + (int)(80 * scale);
        appWindow.MoveAndResize(new RectInt32(left, top, width, height));
    }

    private static void CollapseTitleBar(AppWindow appWindow)
    {
        if (!appWindow.TitleBar.ExtendsContentIntoTitleBar)
            return;

        try
        {
            appWindow.TitleBar.ExtendsContentIntoTitleBar = false;
        }
        catch
        {
            // Best effort.
        }
    }

    private static void ClearWindowDragRegions(Window window, AppWindow appWindow)
    {
        try
        {
            if (window.Content is FrameworkElement root)
            {
                var nonClient = InputNonClientPointerSource.GetForWindowId(appWindow.Id);
                nonClient.ClearRegionRects(NonClientRegionKind.Caption);
                nonClient.SetRegionRects(NonClientRegionKind.Caption, Array.Empty<RectInt32>());
                _ = root;
            }
        }
        catch
        {
            // Best effort.
        }
    }
}
