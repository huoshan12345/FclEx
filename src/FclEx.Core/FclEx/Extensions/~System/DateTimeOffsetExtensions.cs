namespace FclEx.Extensions;

public static class DateTimeOffsetExtensions
{
    public static readonly TimeZoneInfo CnTimeZone = TimeZoneInfo.CreateCustomTimeZone("China", TimeSpan.FromHours(8), null, null);

    public static DateTimeOffset SetOffset(this DateTimeOffset time, TimeSpan offset)
    {
        return new DateTimeOffset(time.DateTime, offset);
    }

    public static DateTimeOffset SetCnOffset(this DateTimeOffset time)
    {
        return time.SetOffset(CnTimeZone.BaseUtcOffset);
    }

    public static DateTimeOffset ToCnTime(this DateTimeOffset time)
    {
        return time.ToOffset(CnTimeZone.BaseUtcOffset);
    }
}
