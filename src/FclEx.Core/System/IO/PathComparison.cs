namespace System.IO;

public enum PathComparison
{
    /// <summary>
    /// Performs a case-sensitive comparison using ordinal rules.
    /// </summary>
    CaseSensitive,

    /// <summary>
    /// Performs a case-insensitive comparison using ordinal rules.
    /// </summary>
    CaseInsensitive,

    /// <summary>
    /// Uses the default behavior of the current operating system
    /// (case-insensitive on Windows, case-sensitive on others).
    /// </summary>
    Auto,
}