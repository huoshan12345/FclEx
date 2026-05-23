namespace System;

[Flags]
public enum SplitOptions
{
    /// <summary>
    /// Do not transform the results. This is the default behavior.
    /// </summary>
    None = 0,

    /// <summary>
    /// Remove empty (zero-length) substrings from the result.
    /// </summary>
    /// <remarks>
    /// If <see cref="RemoveEmptyEntries"/> and <see cref="TrimEntries"/> are specified together,
    /// then substrings that consist only of whitespace characters are also removed from the result.
    /// </remarks>
    RemoveEmptyEntries = 1,

    /// <summary>
    /// Trim whitespace from each substring in the result.
    /// </summary>
    TrimEntries = 2,

    /// <summary>
    /// Trims each substring and excludes empty results.
    /// Equivalent to <see cref="RemoveEmptyEntries"/> | <see cref="TrimEntries"/>.
    /// </summary>
    TrimAndRemoveEmpty = RemoveEmptyEntries | TrimEntries
}

public static class SplitOptionsExtensions
{
    public static StringSplitOptions ToStringSplitOptions(this SplitOptions options)
    {
        return (StringSplitOptions)options;
    }

    public static SplitOptions ToSplitOptions(this StringSplitOptions options)
    {
        return (SplitOptions)options;
    }
}
