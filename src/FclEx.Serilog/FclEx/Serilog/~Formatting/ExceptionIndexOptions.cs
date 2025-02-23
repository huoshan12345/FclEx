namespace FclEx.Serilog;

/// <summary>
/// Specifies the options for including exception indexes in log messages.
/// These options control when and how exception indexes are written, depending on the context of the exception chain.
/// </summary>
[Flags]
public enum ExceptionIndexOptions
{
    None = 0,

    /// <summary>
    /// Include the indexes of exceptions in the log message.
    /// </summary>
    Include = 1 << 0,

    /// <summary>
    /// Include the indexes of exceptions in the log message only if there are multiple exceptions.
    /// </summary>
    IncludeForMultiple = 1 << 1,

    /// <summary>
    /// Include the indexes of exceptions in the log message only if the exceptions form a multi-branched structure.
    /// </summary>
    IncludeForMultiBranched = 1 << 2,

    /// <summary>
    /// Default option: include indexes for multiple exceptions and multi-branched structures.
    /// </summary>
    Default = Include | IncludeForMultiple | IncludeForMultiBranched,
}