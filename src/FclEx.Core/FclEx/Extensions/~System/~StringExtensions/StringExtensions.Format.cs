namespace FclEx.Extensions;

partial class StringExtensions
{
    [MethodImpl(AggressiveInlining)]
    public static string Format(this string str, params object[] args) => string.Format(str, args);

    /// <summary>
    /// Limits the content portion of a string to <paramref name="maxContentLength"/> characters.
    /// </summary>
    /// <remarks>
    /// When <paramref name="appendTrailingDots"/> is <see langword="true"/> and truncation occurs, the returned value
    /// contains the retained content followed by <c>...</c>, so its total length exceeds <paramref name="maxContentLength"/>
    /// by three characters.
    /// </remarks>
    public static string Truncate(this string? str, int maxContentLength, bool appendTrailingDots = true)
    {
        if (maxContentLength <= 0)
            return string.Empty;

        if (str.IsNullOrEmpty() || maxContentLength >= str.Length)
            return str ?? string.Empty;

        var sub = str[..maxContentLength];
        return appendTrailingDots
            ? sub + "..."
            : sub;
    }
}
