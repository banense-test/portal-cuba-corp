namespace PortalCubaCorp.Domain;

/// <summary>
/// Value object representing a date range for month-based queries.
/// Used by IClockingService.GetHistory, GetAllClockings, ExportCsv.
/// </summary>
public readonly record struct DateRange(DateTime Start, DateTime End)
{
    /// <summary>
    /// Creates a DateRange covering a specific month in a specific year.
    /// </summary>
    public static DateRange ForMonth(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddTicks(-1);
        return new DateRange(start, end);
    }

    /// <summary>
    /// Creates a DateRange covering the current month.
    /// </summary>
    public static DateRange CurrentMonth()
    {
        var now = DateTime.UtcNow;
        return ForMonth(now.Year, now.Month);
    }
}