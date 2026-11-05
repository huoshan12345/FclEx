namespace FclEx.Serilog;

/// <summary>Adds operation-error logging, source contexts, and Serilog-aware Microsoft logging scopes.</summary>
public static class LoggerExtensions
{
    /// <summary>Writes an Error event describing a failed action, retaining the exception and structured title/message properties.</summary>
    /// <param name="logger">The Serilog logger receiving the event.</param>
    /// <param name="title">The action description captured in the Title property.</param>
    /// <param name="ex">The exception attached to the event; its Message is also captured in Error.</param>
    public static void ActionError(this ILogger logger, string title, Exception ex)
    {
        logger.Error(ex, "Failed to execute {Title} due to {Error}", title, ex.Message);
    }

    /// <summary>Creates a context logger whose SourceContext is the supplied string.</summary>
    /// <param name="logger">The parent Serilog logger.</param>
    /// <param name="name">The source/category name used for source enrichment and level overrides.</param>
    /// <returns>A context logger retaining the parent's pipeline and ownership.</returns>
    public static ILogger ForContext(this ILogger logger, string name)
    {
        return logger.ForContext(Constants.SourceContext, name);
    }

    /// <summary>Begins a property scope with Serilog's optional object destructuring convention.</summary>
    /// <param name="logger">The Microsoft logger connected to a Serilog provider.</param>
    /// <param name="name">The nonempty structured property name.</param>
    /// <param name="value">The value to attach to the scope.</param>
    /// <param name="destructureObjects">Whether to prefix the name with @ so Serilog captures the object's structure.</param>
    /// <returns>A scope group that releases all pushed properties when disposed.</returns>
    /// <remarks>Other providers may treat the prefixed name as a literal property name.</remarks>
    public static LoggerPropertyScope Properties(this Microsoft.Extensions.Logging.ILogger logger, string name, object? value, bool destructureObjects = false)
    {
        return logger.Properties().Push(name, value, destructureObjects);
    }
}
