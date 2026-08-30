using System.IO;
using System.Text.Json;
using nineth1ngs.Models;

namespace nineth1ngs.Services;

public sealed class Th1ngOrderService
{
    private readonly string filePath;

    public Th1ngOrderService()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
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

            return JsonSerializer.Deserialize<List<string>>(json)
                   ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<Th1ng> openTh1ngs)
    {
        var currentOpenIds = openTh1ngs
            .Select(th1ng => th1ng.Id.ToString())
            .Where(id => id.Length > 0)
            .ToList();

        var savedOrder = Load()
            .Distinct()
            .ToList();

        if (savedOrder.Count == 0)
        {
            Write(currentOpenIds);
            return;
        }

        var currentOpenIdSet = currentOpenIds.ToHashSet();
        var savedIdSet = savedOrder.ToHashSet();

        var existingOpenIds = currentOpenIds
            .Where(savedIdSet.Contains)
            .ToList();

        var existingOpenPositions = savedOrder
            .Select((id, index) => new
            {
                Id = id,
                Index = index
            })
            .Where(item => currentOpenIdSet.Contains(item.Id))
            .Select(item => item.Index)
            .ToList();

        for (var i = 0;
             i < existingOpenPositions.Count &&
             i < existingOpenIds.Count;
             i++)
        {
            savedOrder[existingOpenPositions[i]] =
                existingOpenIds[i];
        }

        foreach (var newId in currentOpenIds
                     .Where(id => !savedIdSet.Contains(id)))
        {
            savedOrder.Add(newId);
        }

        Write(savedOrder);
    }

    public void Remove(int th1ngId)
    {
        var id = th1ngId.ToString();

        var savedOrder = Load()
            .Where(savedId => savedId != id)
            .Distinct()
            .ToList();

        Write(savedOrder);
    }

    private void Write(IReadOnlyList<string> ids)
    {
        var json = JsonSerializer.Serialize(
            ids,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(
            filePath,
            json);
    }
}
