using nineth1ngs.Models;

namespace nineth1ngs.Tests;

public sealed class Th1ngTimerTests
{
    [Fact]
    public void GetElapsedSeconds_IncludesRunningTime()
    {
        var startedAt = new DateTime(2026, 8, 28, 12, 0, 0, DateTimeKind.Utc);
        var th1ng = new Th1ng
        {
            ElapsedSeconds = 10,
            TimerStartedAt = startedAt
        };

        Assert.Equal(25, th1ng.GetElapsedSeconds(startedAt.AddSeconds(15)));
    }

    [Fact]
    public void ElapsedTimeText_FormatsLongDurationsWithoutWrappingHours()
    {
        var th1ng = new Th1ng { ElapsedSeconds = 25 * 60 * 60 + 61 };

        Assert.Equal("25:01:01", th1ng.ElapsedTimeText);
    }

    [Fact]
    public void IsTimerRunning_IsFalseWhenTimerIsPaused()
    {
        var th1ng = new Th1ng { ElapsedSeconds = 90 };

        Assert.False(th1ng.IsTimerRunning);
        Assert.Equal("1:30", th1ng.ElapsedTimeText);
    }
}