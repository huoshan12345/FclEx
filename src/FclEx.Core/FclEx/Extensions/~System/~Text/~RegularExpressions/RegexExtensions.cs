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

    /// <summary>Attempts to get the value of a captured group from the first match.</summary>
    /// <param name="regex">The regular expression to evaluate.</param>
    /// <param name="input">The input to match. A <see langword="null"/> value is treated as an empty string.</param>
    /// <param name="groupIndex">The zero-based group index.</param>
    /// <param name="value">The group value when the method returns <see langword="true"/>; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the input matches and <paramref name="groupIndex"/> identifies an existing group; otherwise, <see langword="false"/>.</returns>
    public static bool TryMatch(this Regex regex, string? input, int groupIndex, [NotNullWhen(true)] out string? value)
    {
        var match = regex.Match(input ?? string.Empty);
        if (match.Success && (uint)groupIndex < (uint)match.Groups.Count)
        {
            value = match.Groups[groupIndex].Value;
            return true;
        }

        value = null;
        return false;
    }
    
    public static string Replace<T>(this Capture capture, string input, Func<string, T?> evaluator)
    {
        var replacement = evaluator(capture.Value)?.ToString() ?? string.Empty;
        if (replacement == capture.Value)
            return input;

        var str = input[..capture.Index] + replacement + input[(capture.Index + capture.Length)..];
        return str;
    }

    public static string Replace<T>(this Regex regex, string input, int groupIndex, Func<string, T?> evaluator, Func<string, string> onMismatch)
    {
        return regex.TryMatch(input, out var match)
            ? match.Groups[groupIndex].Replace(input, evaluator)
            : onMismatch(input);
    }

    public static string Replace<T>(this Regex regex, string input, Func<Match, T?> evaluator)
    {
        return regex.Replace(input, m => evaluator(m)?.ToString() ?? string.Empty);
    }
}
