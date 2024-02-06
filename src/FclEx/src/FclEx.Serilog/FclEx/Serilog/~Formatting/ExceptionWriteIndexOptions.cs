namespace FclEx.Serilog;

[Flags]
public enum ExceptionWriteIndexOptions
{
    None = 0,

    /// <summary>
    /// Write the indexes of exceptions to log message.
    /// </summary>
    Write = 1 << 0,

    /// <summary>
    /// Write the indexes of exceptions to log message only when there are more than one exception.
    /// </summary>
    WriteOnlyForMultiple = 1 << 1,

    /// <summary>
    /// Write the indexes of exceptions to log message only when the structure of exceptions is a multi-branched tree.
    /// </summary>
    WriteOnlyForMultiBranched = 1 << 2,

    Default = Write | WriteOnlyForMultiple | WriteOnlyForMultiBranched,
}