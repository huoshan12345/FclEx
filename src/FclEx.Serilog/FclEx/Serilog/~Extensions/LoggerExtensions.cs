namespace FclEx.Serilog;

public static class LoggerExtensions
{
    public static void ActionError(this ILogger logger, string title, Exception ex)
    {
        logger.Error(ex, "Failed to execute {Title} due to {Error}", title, ex.Message);
    }

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
