using KmTimer.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace KmTimer.Controls;

public sealed partial class HexColorPickerField : UserControl
{
    private bool _syncing;

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(HexColorPickerField),
            new PropertyMetadata(string.Empty, OnHeaderChanged));

    public static readonly DependencyProperty SelectedHexProperty =
        DependencyProperty.Register(
            nameof(SelectedHex),
            typeof(string),
            typeof(HexColorPickerField),
            new PropertyMetadata("#FFFFFF", OnSelectedHexChanged));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string SelectedHex
    {
        get => (string)GetValue(SelectedHexProperty);
        set => SetValue(SelectedHexProperty, value);
    }

    public bool HasHeader => !string.IsNullOrWhiteSpace(Header);

    public HexColorPickerField()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyColorFromHex();
    }

    private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HexColorPickerField field)
            field.Bindings.Update();
    }

    private static void OnSelectedHexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HexColorPickerField field)
            field.ApplyColorFromHex();
    }

    private void ApplyColorFromHex()
    {
        if (_syncing)
            return;

        _syncing = true;
        var color = ColorParse.ToColor(SelectedHex, Color.FromArgb(255, 255, 255, 255));
        Picker.Color = color;
        Swatch.Fill = new SolidColorBrush(color);
        _syncing = false;
    }

    private void Picker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        if (_syncing)
            return;

        _syncing = true;
        var color = args.NewColor;
        SelectedHex = ColorParse.ToHex(color);
        Swatch.Fill = new SolidColorBrush(color);
        _syncing = false;
    }
}
