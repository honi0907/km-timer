using KmTimer.Models;
using KmTimer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace KmTimer.Controls;

public sealed partial class DisplaySurface : UserControl
{
    public static readonly DependencyProperty IsDraftPreviewProperty =
        DependencyProperty.Register(
            nameof(IsDraftPreview),
            typeof(bool),
            typeof(DisplaySurface),
            new PropertyMetadata(false, OnIsDraftPreviewChanged));

    private MainViewModel? _subscribed;

    public bool IsDraftPreview
    {
        get => (bool)GetValue(IsDraftPreviewProperty);
        set => SetValue(IsDraftPreviewProperty, value);
    }

    public DisplaySurface()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += (_, _) => Unsubscribe();
    }

    private static void OnIsDraftPreviewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplaySurface surface)
            surface.ApplyLayout();
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        Unsubscribe();
        if (args.NewValue is MainViewModel vm)
        {
            _subscribed = vm;
            vm.PropertyChanged += ViewModel_PropertyChanged;
            ApplyLayout();
            UpdateBlink(vm.TimerBlink);
            UpdateBgBlink(vm.IsKanpeBgBlinking);
        }
    }

    private void Unsubscribe()
    {
        if (_subscribed is null)
            return;
        _subscribed.PropertyChanged -= ViewModel_PropertyChanged;
        _subscribed = null;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not MainViewModel vm)
            return;

        switch (e.PropertyName)
        {
            case nameof(MainViewModel.TimerBlink):
                UpdateBlink(vm.TimerBlink);
                break;
            case nameof(MainViewModel.IsKanpeBgBlinking):
                UpdateBgBlink(vm.IsKanpeBgBlinking);
                break;
            case nameof(MainViewModel.TimerText):
            case nameof(MainViewModel.TimerBrush):
            case nameof(MainViewModel.ClockText):
            case nameof(MainViewModel.ClockVisible):
            case nameof(MainViewModel.TimerSize):
            case nameof(MainViewModel.KanpeFontSize):
            case nameof(MainViewModel.ClockSize):
            case nameof(MainViewModel.ClockPosition):
            case nameof(MainViewModel.DisplayKanpeText):
            case nameof(MainViewModel.KanpeVisible):
            case nameof(MainViewModel.DraftKanpeText):
            case nameof(MainViewModel.IsKanpeDraftVisible):
                ApplyLayout();
                break;
        }
    }

    private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e) => ApplyLayout();

    private void ApplyLayout()
    {
        if (DataContext is not MainViewModel vm)
            return;

        var width = RootGrid.ActualWidth;
        if (width <= 0)
            return;

        if (IsDraftPreview)
        {
            Grid.SetRow(KanpeBorder, 1);
            Grid.SetRowSpan(KanpeBorder, 1);
            KanpeBorder.VerticalAlignment = VerticalAlignment.Stretch;

            ClockTextBlock.Visibility = vm.ClockVisible ? Visibility.Visible : Visibility.Collapsed;
            TimerTextBlock.Visibility = Visibility.Visible;
            TimerTextBlock.FontSize = Math.Max(24, width * (vm.TimerSize / 100.0));
            KanpeTextBlock.FontSize = Math.Max(16, vm.KanpeFontSize * 16);
            KanpeTextBlock.Text = vm.DraftKanpeText;
            ClockTextBlock.FontSize = Math.Max(12, vm.ClockSize * 16);
            KanpeBorder.Visibility = vm.IsKanpeDraftVisible ? Visibility.Visible : Visibility.Collapsed;

            Canvas.SetZIndex(ClockTextBlock, 10);
            PositionClock(vm.ClockPosition);
            return;
        }

        Grid.SetRow(KanpeBorder, 1);
        Grid.SetRowSpan(KanpeBorder, 1);
        KanpeBorder.VerticalAlignment = VerticalAlignment.Stretch;

        ClockTextBlock.Visibility = vm.ClockVisible ? Visibility.Visible : Visibility.Collapsed;
        TimerTextBlock.Visibility = Visibility.Visible;

        TimerTextBlock.FontSize = Math.Max(24, width * (vm.TimerSize / 100.0));
        KanpeTextBlock.FontSize = Math.Max(16, vm.KanpeFontSize * 16);
        KanpeTextBlock.Text = vm.DisplayKanpeText;
        ClockTextBlock.FontSize = Math.Max(12, vm.ClockSize * 16);

        var hasKanpe = vm.KanpeVisible && !string.IsNullOrWhiteSpace(vm.DisplayKanpeText);
        KanpeBorder.Visibility = hasKanpe ? Visibility.Visible : Visibility.Collapsed;

        Canvas.SetZIndex(ClockTextBlock, 10);
        PositionClock(vm.ClockPosition);
    }

    private void PositionClock(ClockPosition position)
    {
        ClockTextBlock.HorizontalAlignment = position is ClockPosition.TopRight or ClockPosition.BottomRight
            ? HorizontalAlignment.Right
            : HorizontalAlignment.Left;
        ClockTextBlock.VerticalAlignment = position is ClockPosition.BottomLeft or ClockPosition.BottomRight
            ? VerticalAlignment.Bottom
            : VerticalAlignment.Top;
        ClockTextBlock.Margin = new Thickness(20);
    }

    private void UpdateBlink(bool blink)
    {
        if (blink)
            BlinkStoryboard.Begin();
        else
        {
            BlinkStoryboard.Stop();
            TimerTextBlock.Opacity = 1;
            TimerScale.ScaleX = 1;
            TimerScale.ScaleY = 1;
        }
    }

    private void UpdateBgBlink(bool blink)
    {
        if (blink)
            BgBlinkStoryboard.Begin();
        else
        {
            BgBlinkStoryboard.Stop();
            BgBlinkOverlay.Opacity = 0;
        }
    }
}
