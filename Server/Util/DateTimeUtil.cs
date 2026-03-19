namespace Server.Util;

public static class DateTimeUtil
{
    /// <summary>
    /// We do not have microsecond/nanosecond precision on the database.
    /// So, this truncates microsecond/nanosecond.
    /// </summary>
    public static DateTime UtcNowMs
    {
        get
        {
            var result = DateTime.UtcNow;
            result = result.AddMicroseconds(-result.Microsecond);
            result = result.Add(-Nanoseconds(result.Nanosecond));
            return result;
        }
    }

    private const double TicksPerNanosecond = TimeSpan.TicksPerMicrosecond / 1000d;
    private static TimeSpan Nanoseconds(this long nanoseconds)
    {
        return TimeSpan.FromTicks((long)Math.Round(nanoseconds * TicksPerNanosecond));
    }
}
