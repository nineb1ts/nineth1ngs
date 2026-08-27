using nineth1ngs.Services;

namespace nineth1ngs.Tests;

public sealed class TimestampFormatterTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 27, 16, 42, 0, DateTimeKind.Utc);

    [Fact]
    public void Format_UsesHeuteForTheCurrentLocalDate()
    {
        var createdAtUtc = NowUtc.AddMinutes(-20);

        var result = TimestampFormatter.Format(createdAtUtc, NowUtc);

        Assert.StartsWith("Heute, ", result);
    }

    [Fact]
    public void Format_UsesGesternForThePreviousLocalDate()
    {
        var nowLocal = NowUtc.ToLocalTime();
        var createdAtUtc = nowLocal.Date.AddDays(-1).AddHours(21).ToUniversalTime();

        var result = TimestampFormatter.Format(createdAtUtc, NowUtc);

        Assert.StartsWith("Gestern, ", result);
    }

    [Fact]
    public void Format_UsesFullDateForOlderEntries()
    {
        var createdAtUtc = NowUtc.AddDays(-3);

        var result = TimestampFormatter.Format(createdAtUtc, NowUtc);

        Assert.Equal(createdAtUtc.ToLocalTime().ToString("dd.MM.yyyy, HH:mm"), result);
    }
}