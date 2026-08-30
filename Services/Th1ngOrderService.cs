using System.Text.Json;
using System.IO;
using nineth1ngs.Models;

namespace nineth1ngs.Services;

public sealed class Th1ngOrderService
{
    private readonly string filePath;

    public Th1ngOrderService()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "nineth1ngs");

        Directory.CreateDirectory(directory);

        filePath = Path.Combine(
            directory,
            "th1ng-order.json");
    }

    public IReadOnlyList<string> Load()
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<Th1ng> th1ngs)
    {
        var ids = th1ngs
            .Select(th1ng => th1ng.Id.ToString() ?? string.Empty)
            .Where(id => id.Length > 0)
            .ToList();

        var json = JsonSerializer.Serialize(
            ids,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(filePath, json);
    }
}
