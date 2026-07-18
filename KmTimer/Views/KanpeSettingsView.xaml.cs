using KmTimer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace KmTimer.Views;

public sealed partial class KanpeSettingsView : UserControl
{
    public MainViewModel ViewModel => App.MainViewModel;

    public KanpeSettingsView()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateSendButtonStyles();
            UpdateSelectedSlotBorders();
            UpdateBgBlinkButtonStyle();
        };
        Unloaded += (_, _) => ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.ActiveKanpeSlot)
            or nameof(MainViewModel.IsKanpeSlot1Sending)
            or nameof(MainViewModel.IsKanpeSlot2Sending)
            or nameof(MainViewModel.IsKanpeSlot3Sending)
            or nameof(MainViewModel.IsKanpeSlot4Sending))
        {
            UpdateSendButtonStyles();
        }

        if (e.PropertyName is nameof(MainViewModel.SelectedKanpeSlot)
            or nameof(MainViewModel.IsKanpeSlot1Selected)
            or nameof(MainViewModel.IsKanpeSlot2Selected)
            or nameof(MainViewModel.IsKanpeSlot3Selected)
            or nameof(MainViewModel.IsKanpeSlot4Selected))
        {
            UpdateSelectedSlotBorders();
        }

        if (e.PropertyName is nameof(MainViewModel.IsKanpeBgBlinking)
            or nameof(MainViewModel.KanpeBgBlinkLabel))
        {
            UpdateBgBlinkButtonStyle();
        }
    }

    private void KanpeBgBlinkButton_Click(object sender, RoutedEventArgs e) =>
        ViewModel.ToggleKanpeBgBlinkCommand.Execute(null);

    private void KanpeSendButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string tag || !int.TryParse(tag, out var slot))
            return;

        ViewModel.ToggleKanpeSendCommand.Execute(tag);
        ViewModel.BeginKanpePreview(slot);
        GetKanpeInput(slot)?.Focus(FocusState.Programmatic);
    }

    private TextBox? GetKanpeInput(int slot) => slot switch
    {
        1 => KanpeInput1,
        2 => KanpeInput2,
        3 => KanpeInput3,
        4 => KanpeInput4,
        _ => null
    };

    private void UpdateSendButtonStyles()
    {
        ApplySendButtonStyle(KanpeSend1, ViewModel.IsKanpeSlot1Sending);
        ApplySendButtonStyle(KanpeSend2, ViewModel.IsKanpeSlot2Sending);
        ApplySendButtonStyle(KanpeSend3, ViewModel.IsKanpeSlot3Sending);
        ApplySendButtonStyle(KanpeSend4, ViewModel.IsKanpeSlot4Sending);
    }

    private void UpdateSelectedSlotBorders()
    {
        ApplySlotBorder(KanpeCard1, ViewModel.IsKanpeSlot1Selected);
        ApplySlotBorder(KanpeCard2, ViewModel.IsKanpeSlot2Selected);
        ApplySlotBorder(KanpeCard3, ViewModel.IsKanpeSlot3Selected);
        ApplySlotBorder(KanpeCard4, ViewModel.IsKanpeSlot4Selected);
    }

    private static void ApplySlotBorder(Border border, bool isSelected)
    {
        border.BorderThickness = isSelected ? new Thickness(2) : new Thickness(1);
        border.BorderBrush = isSelected
            ? (Brush)Application.Current.Resources["AccentFillColorDefaultBrush"]
            : (Brush)Application.Current.Resources["ControlStrokeColorDefaultBrush"];
    }

    private void ApplySendButtonStyle(Button button, bool isSending)
    {
        button.Style = isSending
            ? (Style)Resources["KanpeReleaseButtonStyle"]
            : (Style)Resources["KanpeSendButtonStyle"];
    }

    private void UpdateBgBlinkButtonStyle()
    {
        if (ViewModel.IsKanpeBgBlinking)
        {
            KanpeBgBlinkButton.Style = (Style)Resources["KanpeReleaseButtonStyle"];
            KanpeBgBlinkButton.ClearValue(FrameworkElement.WidthProperty);
            KanpeBgBlinkButton.Height = 40;
            KanpeBgBlinkButton.MinWidth = 120;
        }
        else
        {
            KanpeBgBlinkButton.Style = (Style)Application.Current.Resources["AccentButtonStyle"];
            KanpeBgBlinkButton.ClearValue(FrameworkElement.WidthProperty);
            KanpeBgBlinkButton.ClearValue(FrameworkElement.HeightProperty);
            KanpeBgBlinkButton.ClearValue(FrameworkElement.MinWidthProperty);
        }
    }

    private void KanpeInput1_GotFocus(object sender, RoutedEventArgs e) => ViewModel.BeginKanpePreview(1);
    private void KanpeInput2_GotFocus(object sender, RoutedEventArgs e) => ViewModel.BeginKanpePreview(2);
    private void KanpeInput3_GotFocus(object sender, RoutedEventArgs e) => ViewModel.BeginKanpePreview(3);
    private void KanpeInput4_GotFocus(object sender, RoutedEventArgs e) => ViewModel.BeginKanpePreview(4);

    private void KanpeInput1_TextChanged(object sender, TextChangedEventArgs e) =>
        ViewModel.UpdateKanpePreview(1, KanpeInput1.Text);

    private void KanpeInput2_TextChanged(object sender, TextChangedEventArgs e) =>
        ViewModel.UpdateKanpePreview(2, KanpeInput2.Text);

    private void KanpeInput3_TextChanged(object sender, TextChangedEventArgs e) =>
        ViewModel.UpdateKanpePreview(3, KanpeInput3.Text);

    private void KanpeInput4_TextChanged(object sender, TextChangedEventArgs e) =>
        ViewModel.UpdateKanpePreview(4, KanpeInput4.Text);
}
