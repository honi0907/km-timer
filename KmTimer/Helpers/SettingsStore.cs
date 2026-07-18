using System.Text.Json;
using KmTimer.Models;

namespace KmTimer.Helpers;

internal static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static string RootDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KmTimer");

    private static string PresetPath(int index) =>
        Path.Combine(RootDirectory, $"preset-{index}.json");

    public static PresetSettings? LoadPreset(int index)
    {
        try
        {
            var path = PresetPath(index);
            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PresetSettings>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static void SavePreset(int index, PresetSettings settings)
    {
        Directory.CreateDirectory(RootDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(PresetPath(index), json);
    }

    public static bool PresetExists(int index) => File.Exists(PresetPath(index));
}
