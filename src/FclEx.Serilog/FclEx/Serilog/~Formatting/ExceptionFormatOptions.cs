namespace FclEx.Serilog;

/// <summary>
/// Controls exception descriptions, message truncation, stack-frame text, type names, and tree indexes in JsonFormatter output.
/// </summary>
public class ExceptionFormatOptions
{
    /// <summary>
    /// The maximum retained UTF-16 message content length, or null for unlimited content.
    /// Truncated positive-length content is followed by three dots, which are outside this limit.
    /// Zero or negative values produce empty message content. The default is null.
    /// </summary>
    public int? MaxMessageLength { get; set; } = null;
    /// <summary>
    /// Removes the first matched parameter list from each retained stack-frame line. Defaults to true.
    /// </summary>
    public bool OmitParametersInStackTrace { get; set; } = true;
    /// <summary>
    /// Omits an exception message already contained in the rendered log message, using a case-sensitive substring check before truncation. Defaults to true.
    /// </summary>
    public bool OmitMessageIfExists { get; set; } = true;
    /// <summary>
    /// Specifies whether to use the simple name or the full name for the exception type.
    /// <br/> <see langword="true" /> uses <see cref="TypeExtensions.SimpleName"/>,
    /// <br/> <see langword="false" /> uses <see cref="Type.FullName"/>.
    /// </summary>
    public bool UseSimpleTypeName { get; set; } = true;
    /// <summary>
    /// Determines when exception indexes and parent indexes are included. Defaults to trees with multiple branches.
    /// </summary>
    public ExceptionIndexPolicy IndexPolicy { get; set; } = ExceptionIndexPolicy.WhenBranched;
    /// <summary>
    /// A shared, mutable options instance for callers that explicitly choose shared configuration.
    /// New JsonFormatterOptions instances create independent exception options instead of using this instance.
    /// </summary>
    public static readonly ExceptionFormatOptions Default = new();
}
