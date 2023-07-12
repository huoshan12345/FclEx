namespace FclEx.Utils;

public static class CommonRegex
{
    public static Regex HostPort { get; } = new(@"([^:]+)(?::(\d+))?", RegexOptions.Compiled);
    public static Regex Ipv6HostPort { get; } = new(@"\[[^\[^\]]+\](?::(\d+))?", RegexOptions.Compiled);
    public static Regex Scheme { get; } = new(@"(\S+)://", RegexOptions.Compiled);
}