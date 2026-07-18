using KmTimer.Models;
using KmTimer.Services;
using KmTimer.ViewModels;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KmTimer.Views;

public sealed partial class SystemSettingsView : UserControl
{
    public MainViewModel ViewModel => App.MainViewModel;

    public SystemSettingsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ClockPositionBox.SelectedItem = ViewModel.ClockPositionOptions
            .FirstOrDefault(o => o.Value == ViewModel.ClockPosition);
    }

    private void ClockPositionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ClockPositionBox.SelectedItem is ClockPositionOption option)
            ViewModel.ClockPosition = option.Value;
    }

    private async void OnlineUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        var dispatcher = DispatcherQueue.GetForCurrentThread() ?? App.DispatcherQueue;
        OnlineUpdateButton.IsEnabled = false;
        try
        {
            await OnlineUpdateUiHelper.RunAsync(
                XamlRoot,
                status => RunOnUi(dispatcher, () => UpdateStatusText.Text = status));
        }
        catch (Exception ex)
        {
            RunOnUi(dispatcher, () => UpdateStatusText.Text = $"更新に失敗しました: {ex.Message}");
        }
        finally
        {
            RunOnUi(dispatcher, () => OnlineUpdateButton.IsEnabled = true);
        }
    }

    private static void RunOnUi(DispatcherQueue dispatcher, Action action)
    {
        if (dispatcher.HasThreadAccess)
        {
            action();
            return;
        }

        dispatcher.TryEnqueue(() => action());
    }
}
