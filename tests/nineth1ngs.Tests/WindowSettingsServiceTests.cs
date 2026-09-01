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

    [Fact]
    public void IsValidMiniPosition_RejectsNonFiniteCoordinates()
    {
        var settings = new WindowSettings
        {
            MiniLeft = double.NaN,
            MiniTop = 20
        };

        Assert.False(WindowSettingsService.IsValidMiniPosition(settings));
    }

    [Fact]
    public void SnapToWorkingArea_SnapsEachEdgeWithinDistance()
    {
        var workingArea = new Rect(100, 50, 800, 500);
        var windowSize = new Size(200, 100);

        var position = MiniModeLayoutService.SnapToWorkingArea(
            new Point(105, 445),
            windowSize,
            workingArea);

        Assert.Equal(100, position.X);
        Assert.Equal(450, position.Y);
    }

    [Fact]
    public void SnapToWorkingArea_ClampsNegativeMonitorCoordinates()
    {
        var workingArea = new Rect(-1920, 0, 1920, 1080);
        var windowSize = new Size(360, 86);

        var position = MiniModeLayoutService.SnapToWorkingArea(
            new Point(-2000, -40),
            windowSize,
            workingArea);

        Assert.Equal(-1920, position.X);
        Assert.Equal(0, position.Y);
    }

    [Fact]
    public void GetQuickInputPosition_UsesAboveWhenBelowDoesNotFit()
    {
        var miniBounds = new Rect(300, 700, 440, 86);
        var inputSize = new Size(440, 52);
        var workingArea = new Rect(0, 0, 1200, 768);

        var position = MiniModeLayoutService.GetQuickInputPosition(
            miniBounds,
            inputSize,
            workingArea);

        Assert.Equal(300, position.X);
        Assert.Equal(644, position.Y);
    }

    private static string CreateTemporarySettingsFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"nineth1ngs-tests-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        return path;
    }
}