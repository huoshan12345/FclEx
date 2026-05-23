namespace FclEx.Helpers;

public static class DateTimeHelper
{
    public static DateTime FromUnixTimeSeconds(long seconds)
        => DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;

    public static DateTime FromUnixTimeMilliseconds(long milliseconds)
        => DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).DateTime;
}