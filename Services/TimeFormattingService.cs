using nineth1ngs.Models;

namespace nineth1ngs.Services;

public class TimeFormattingService
{
    public string FormatForCopy(
        int elapsedSeconds,
        TimeCopySettings settings)
    {
        var totalMinutes = elapsedSeconds / 60.0;

        var interval = settings.BillingIntervalMinutes;
        var threshold = settings.RoundUpThresholdMinutes;

        var lowerStep = Math.Floor(totalMinutes / interval) * interval;
        var minutesIntoInterval = totalMinutes - lowerStep;

        var roundedMinutes = minutesIntoInterval >= threshold
            ? lowerStep + interval
            : lowerStep;

        var decimalHours = roundedMinutes / 60.0;

        return decimalHours.ToString("0.##");
    }
}