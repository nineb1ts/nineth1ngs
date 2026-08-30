namespace nineth1ngs.Models;

public class TimeCopySettings
{
    public int BillingIntervalMinutes { get; set; } = 15;

    public int RoundUpThresholdMinutes { get; set; } = 8;

    public TimeOutputFormat OutputFormat { get; set; } = TimeOutputFormat.DecimalHours;
}