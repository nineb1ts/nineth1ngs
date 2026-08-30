using nineth1ngs.Models;

namespace nineth1ngs.Services;

public class TimeFormattingService
{
    public string FormatForCopy(
        int elapsedSeconds,
        TimeCopySettings settings)
    {
        var roundedMinutes = RoundMinutes(
            elapsedSeconds,
            settings.BillingIntervalMinutes,
            settings.RoundUpThresholdMinutes);

        return settings.OutputFormat switch
        {
            TimeOutputFormat.DecimalHours =>
                FormatDecimalHours(roundedMinutes),

            TimeOutputFormat.HoursAndMinutes =>
                FormatHoursAndMinutes(roundedMinutes),

            _ => FormatDecimalHours(roundedMinutes)
        };
    }

    private static int RoundMinutes(
        int elapsedSeconds,
        int interval,
        int threshold)
    {
        var totalMinutes = elapsedSeconds / 60.0;

        if (elapsedSeconds > 0 && totalMinutes < interval)
        {
            totalMinutes = interval;
        }

        var lowerStep = Math.Floor(totalMinutes / interval) * interval;
        var minutesIntoInterval = totalMinutes - lowerStep;

        var roundedMinutes = minutesIntoInterval >= threshold
            ? lowerStep + interval
            : lowerStep;

        return (int)roundedMinutes;
    }

    private static string FormatDecimalHours(int minutes)
    {
        var decimalHours = minutes / 60.0;

        return decimalHours.ToString("0.##");
    }

    private static string FormatHoursAndMinutes(int minutes)
    {
        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;

        return $"{hours:00}:{remainingMinutes:00}";
    }
}