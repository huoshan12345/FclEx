namespace FclEx.Logging;

/// <summary>A collected message and its original logging metadata.</summary>
/// <param name="Category">The category supplied when the logger was created.</param>
/// <param name="Level">The message level.</param>
/// <param name="EventId">The event identifier.</param>
/// <param name="Message">The message produced by the log formatter at collection time.</param>
/// <param name="Exception">The original exception, if any.</param>
/// <param name="State">The original log state, which may contain structured message properties.</param>
/// <remarks>State, exceptions, and scope values are retained without deep copying.</remarks>
public sealed record LogEntry(
    string Category,
    LogLevel Level,
    EventId EventId,
    string Message,
    Exception? Exception,
    object? State)
{
    /// <summary>Gets the captured scope stack in outermost-to-innermost order.</summary>
    /// <remarks>The collector copies the stack membership, but retains each scope value by reference.</remarks>
    public IReadOnlyList<object?> Scopes { get; init; } = [];
}
