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

    public static LoggerProperties Properties(this Microsoft.Extensions.Logging.ILogger logger, string name, object? value, bool destructureObjects = false)
    {
        return logger.Properties().Push(name, value, destructureObjects);
    }
}