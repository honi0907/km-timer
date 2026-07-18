using KmTimer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace KmTimer.Views;

public sealed partial class TimerSettingsView : UserControl
{
    public MainViewModel ViewModel => App.MainViewModel;

    public TimerSettingsView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateTransportButtonStyle();
        };
        Unloaded += (_, _) => ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.IsTimerRunning)
            or nameof(MainViewModel.IsTimerPaused))
        {
            UpdateTransportButtonStyle();
        }
    }

    private void UpdateTransportButtonStyle()
    {
        if (ViewModel.IsTimerRunning)
        {
            TimerStartPauseButton.Style = (Style)Resources["TimerStartActiveButtonStyle"];
            TimerStartPauseButton.Content = "⏸ PAUSE";
        }
        else if (ViewModel.IsTimerPaused)
        {
            TimerStartPauseButton.Style = (Style)Resources["TimerPauseActiveButtonStyle"];
            TimerStartPauseButton.Content = "▶ START";
        }
        else
        {
            TimerStartPauseButton.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            TimerStartPauseButton.Content = "▶ / ⏸ START";
        }
    }
}
