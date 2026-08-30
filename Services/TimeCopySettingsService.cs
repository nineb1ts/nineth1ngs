using System.IO;
using System.Text.Json;
using nineth1ngs.Models;

namespace nineth1ngs.Services;

public class TimeCopySettingsService
{
    private readonly string settingsPath;

    public TimeCopySettingsService()
    {
        var appDataPath = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);

        var appFolder = Path.Combine(
            appDataPath,
            "nineth1ngs");

        Directory.CreateDirectory(appFolder);

        settingsPath = Path.Combine(
            appFolder,
            "time-copy-settings.json");
    }

    public TimeCopySettings Load()
    {
        if (!File.Exists(settingsPath))
        {
            return new TimeCopySettings();
        }

        try
        {
            var json = File.ReadAllText(settingsPath);

            return JsonSerializer.Deserialize<TimeCopySettings>(json)
                   ?? new TimeCopySettings();
        }
        catch
        {
            return new TimeCopySettings();
        }
    }

    public void Save(TimeCopySettings settings)
    {
        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(settingsPath, json);
    }
}