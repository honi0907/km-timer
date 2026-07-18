using KmTimer.Models;
using KmTimer.Services;
using KmTimer.ViewModels;
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
        OnlineUpdateButton.IsEnabled = false;
        try
        {
            await OnlineUpdateUiHelper.RunAsync(
                XamlRoot,
                status => UpdateStatusText.Text = status);
        }
        finally
        {
            OnlineUpdateButton.IsEnabled = true;
        }
    }
}
