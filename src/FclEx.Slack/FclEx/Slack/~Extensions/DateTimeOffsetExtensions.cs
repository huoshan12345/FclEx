namespace FclEx.Slack;

public static class DateTimeOffsetExtensions
{
    public static string ToTs(this DateTimeOffset dto)
    {
        return dto.ToUnixTimeSeconds().ToString("f6");
    }
}
