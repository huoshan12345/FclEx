namespace FclEx.Serilog;

/// <summary>
/// Configures the formatting options for exceptions when logging or rendering them.
/// This class provides settings for controlling the message length, stack trace behavior, type name formatting, and more.
/// </summary>
public class ExceptionFormatOptions
{
    /// <summary>
    /// The maximum length of the exception message.
    /// Defaults to <see langword="null" /> if not set.
    /// </summary>
    public int? MaxMessageLength { get; set; } = null;
    /// <summary>
    /// Determines whether parameters are omitted in the stack trace output.
    /// </summary>
    public bool OmitParametersInStackTrace { get; set; } = true;
    /// <summary>
    /// Specifies whether the exception message should be omitted if it already exists in the rendered <see cref="MessageTemplate"/>.
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
