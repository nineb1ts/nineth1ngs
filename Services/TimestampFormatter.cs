using System.Globalization;
using System.Windows.Data;

namespace nineth1ngs.Services;

public static class TimestampFormatter
{
    public static string Format(DateTime createdAtUtc, DateTime nowUtc)
    {
        var createdAtLocal = AsUtc(createdAtUtc).ToLocalTime();
        var nowLocal = AsUtc(nowUtc).ToLocalTime();
        var time = createdAtLocal.ToString("HH:mm", CultureInfo.InvariantCulture);

        if (createdAtLocal.Date == nowLocal.Date)
        {
            return $"Heute, {time}";
        }

        if (createdAtLocal.Date == nowLocal.Date.AddDays(-1))
        {
            return $"Gestern, {time}";
        }

        return createdAtLocal.ToString("dd.MM.yyyy, HH:mm", CultureInfo.InvariantCulture);
    }

    private static DateTime AsUtc(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Utc);
}

public sealed class TimestampDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is DateTime createdAt
            ? TimestampFormatter.Format(createdAt, DateTime.UtcNow)
            : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
