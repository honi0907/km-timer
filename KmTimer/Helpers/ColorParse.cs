using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace KmTimer.Helpers;

internal static class ColorParse
{
    public static Color ToColor(string? hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;

        var value = hex.Trim();
        if (value.StartsWith('#'))
            value = value[1..];

        if (value.Length == 6
            && byte.TryParse(value[..2], System.Globalization.NumberStyles.HexNumber, null, out var r)
            && byte.TryParse(value[2..4], System.Globalization.NumberStyles.HexNumber, null, out var g)
            && byte.TryParse(value[4..6], System.Globalization.NumberStyles.HexNumber, null, out var b))
        {
            return Color.FromArgb(255, r, g, b);
        }

        return fallback;
    }

    public static SolidColorBrush ToBrush(string? hex, Color fallback) =>
        new(ToColor(hex, fallback));

    public static string ToHex(Color color) =>
        $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
