namespace FclEx.Extensions;

public static class RegexExtensions
{
    public static string? Get(this Match m, int index = 0, string? defaultValue = default)
    {
        return m.Success && index >= 0 && index < m.Groups.Count
            ? m.Groups[index].Value
            : defaultValue;
    }

    public static int GetInt(this Match m, int index = 0, int defaultValue = default)
    {
        var s = m.Get(index);
        return s != null && int.TryParse(s, out var i)
            ? i
            : defaultValue;
    }

    public static T? Get<T>(this Regex regex, string? input, Func<Match, T> func, T? defaultValue = default)
    {
        if (input == null)
            return defaultValue;

        var m = regex.Match(input);
        return m.Success
            ? func(m)
            : defaultValue;
    }

    public static string Get(this Regex regex, string? input, int groupIndex = 0, string defaultValue = "")
    {
        return regex.Get(input, m => m.Get(groupIndex)) ?? defaultValue;
    }

    public static bool TryMatch(this Regex regex, string? input, [NotNullWhen(true)] out Match? match)
    {
        if (input != null)
        {
            match = regex.Match(input);
            return match.Success;
        }

        match = null;
        return false;
    }

    public static bool TryMatch(this Regex regex, string? input, int groupIndex, [NotNullWhen(true)] out string? value)
    {
        var match = regex.Match(input ?? string.Empty);
        if (match.Success)
        {
            value = match.Groups[groupIndex].Value;
            return true;
        }

        value = null;
        return false;
    }
}