using nineth1ngs.Models;

namespace nineth1ngs.Tests;

public sealed class Th1ngTimerTests
{
    [Fact]
    public void GetElapsedSeconds_IncludesRunningTime()
    {
        var startedAt = new DateTime(
            2026,
            8,
            28,
            12,
            0,
            0,
            DateTimeKind.Utc);

        var th1ng = new Th1ng
        {
            ElapsedSeconds = 10,
            TimerStartedAt = startedAt
        };

        var result = th1ng.GetElapsedSeconds(
            startedAt.AddSeconds(15));

        Assert.Equal(25, result);
    }

    [Fact]
    public void GetElapsedSeconds_WhenPaused_ReturnsStoredElapsedTime()
    {
        var th1ng = new Th1ng
        {
            ElapsedSeconds = 125,
            TimerStartedAt = null
        };

        var result = th1ng.GetElapsedSeconds(
            new DateTime(
                2026,
                8,
                28,
                12,
                0,
                0,
                DateTimeKind.Utc));

        Assert.Equal(125, result);
    }

    [Fact]
    public void IsTimerRunning_IsTrueWhenTimerHasStartTime()
    {
        var th1ng = new Th1ng
        {
            TimerStartedAt = new DateTime(
                2026,
                8,
                28,
                12,
                0,
                0,
                DateTimeKind.Utc)
        };

        Assert.True(th1ng.IsTimerRunning);
    }

    [Fact]
    public void IsTimerRunning_IsFalseWhenTimerIsPaused()
    {
        var th1ng = new Th1ng
        {
            ElapsedSeconds = 90
        };

        Assert.False(th1ng.IsTimerRunning);
        Assert.Equal("1:30", th1ng.ElapsedTimeText);
    }

    [Theory]
    [InlineData(0, "0:00")]
    [InlineData(1, "0:01")]
    [InlineData(59, "0:59")]
    [InlineData(60, "1:00")]
    [InlineData(61, "1:01")]
    [InlineData(3599, "59:59")]
    public void ElapsedTimeText_FormatsDurationsBelowOneHour(
        int elapsedSeconds,
        string expected)
    {
        var th1ng = new Th1ng
        {
            ElapsedSeconds = elapsedSeconds
        };

        Assert.Equal(expected, th1ng.ElapsedTimeText);
    }

    [Theory]
    [InlineData(3600, "1:00:00")]
    [InlineData(3661, "1:01:01")]
    [InlineData(12 * 60 * 60 + 34 * 60 + 56, "12:34:56")]
    [InlineData(25 * 60 * 60 + 61, "25:01:01")]
    public void ElapsedTimeText_FormatsLongDurationsWithoutWrappingHours(
        int elapsedSeconds,
        string expected)
    {
        var th1ng = new Th1ng
        {
            ElapsedSeconds = elapsedSeconds
        };

        Assert.Equal(expected, th1ng.ElapsedTimeText);
    }
}
