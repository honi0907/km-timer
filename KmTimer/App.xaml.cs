using KmTimer.ViewModels;
using Microsoft.UI.Xaml;

namespace KmTimer;

public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static OutputWindow? OutputWindow { get; private set; }
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static MainViewModel MainViewModel { get; private set; } = null!;

    private static bool _isShuttingDown;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        MainViewModel = new MainViewModel(DispatcherQueue);

        Window = new MainWindow();
        Window.Activate();

        OutputWindow = new OutputWindow();
        OutputWindow.ShowOnDisplay();
    }

    public static void Shutdown()
    {
        if (_isShuttingDown)
            return;

        _isShuttingDown = true;

        try
        {
            OutputWindow?.Close();
        }
        catch
        {
            // Best effort.
        }

        OutputWindow = null;
        Current.Exit();
    }
}
