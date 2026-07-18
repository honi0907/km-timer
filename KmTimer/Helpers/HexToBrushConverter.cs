using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace KmTimer.Helpers;

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var fallback = parameter is string hex
            ? ColorParse.ToColor(hex, Color.FromArgb(255, 15, 23, 42))
            : Color.FromArgb(255, 15, 23, 42);
        return ColorParse.ToBrush(value as string, fallback);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
