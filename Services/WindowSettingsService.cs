using System.IO;
using System.Diagnostics;
using System.Text.Json;
using nineth1ngs.Models;
using System.Windows;

namespace nineth1ngs.Services;

public sealed class WindowSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static string SettingsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "nineth1ngs");

    public static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    private readonly string settingsPath;

    public WindowSettingsService(string? settingsPath = null)
    {
        this.settingsPath = settingsPath ?? SettingsPath;
    }

    public WindowSettings Load()
    {
        try
        {
            if (!File.Exists(settingsPath))
            {
                return new WindowSettings();
            }

            var settings = JsonSerializer.Deserialize<WindowSettings>(
                File.ReadAllText(settingsPath),
                JsonOptions);

            if (settings is null)
            {
                return new WindowSettings();
            }

            if (IsValid(settings) && IsVisible(settings))
            {
                return settings;
            }

            return new WindowSettings
            {
                MiniLeft = settings.MiniLeft,
                MiniTop = settings.MiniTop
            };
        }
        catch (JsonException)
        {
            Debug.WriteLine($"Ignoring invalid window settings at '{settingsPath}'.");
            return new WindowSettings();
        }
        catch (IOException)
        {
            Debug.WriteLine($"Ignoring unreadable window settings at '{settingsPath}'.");
            return new WindowSettings();
        }
    }

    public void Save(WindowSettings settings)
    {
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(settingsPath, json);
    }

    public static bool IsValid(WindowSettings? settings) =>
        settings is not null &&
        double.IsFinite(settings.Width) &&
        double.IsFinite(settings.Height) &&
        double.IsFinite(settings.Left) &&
        double.IsFinite(settings.Top) &&
        settings.Width >= 360 &&
        settings.Height >= 520;

    public static bool IsVisible(WindowSettings settings)
    {
        var virtualScreen = SystemParameters.VirtualScreenWidth > 0 && SystemParameters.VirtualScreenHeight > 0
            ? new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight)
            : SystemParameters.WorkArea;
        var windowBounds = new Rect(settings.Left, settings.Top, settings.Width, settings.Height);

        return virtualScreen.IntersectsWith(windowBounds);
    }

    public static bool IsValidMiniPosition(WindowSettings? settings) =>
        settings?.MiniLeft is double left &&
        settings.MiniTop is double top &&
        double.IsFinite(left) &&
        double.IsFinite(top);
}
