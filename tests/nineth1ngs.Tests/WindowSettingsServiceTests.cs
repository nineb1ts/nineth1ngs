using System.IO;
using System.Windows;
using nineth1ngs.Models;
using nineth1ngs.Services;

namespace nineth1ngs.Tests;

public sealed class WindowSettingsServiceTests
{
    [Fact]
    public void Load_ReturnsDefaultsForCorruptedJson()
    {
        var path = CreateTemporarySettingsFile("{ invalid json");

        var settings = new WindowSettingsService(path).Load();

        Assert.Equal(0, settings.Width);
        Assert.Equal(0, settings.Height);
    }

    [Fact]
    public void IsValid_RejectsWindowBelowMinimumSize()
    {
        var settings = new WindowSettings
        {
            Width = 359,
            Height = 520
        };

        Assert.False(WindowSettingsService.IsValid(settings));
    }

    [Fact]
    public void IsVisible_RejectsWindowOutsideVirtualScreen()
    {
        var settings = new WindowSettings
        {
            Width = 480,
            Height = 680,
            Left = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth + 100,
            Top = SystemParameters.VirtualScreenTop + 100
        };

        Assert.False(WindowSettingsService.IsVisible(settings));
    }

    private static string CreateTemporarySettingsFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"nineth1ngs-tests-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        return path;
    }
}