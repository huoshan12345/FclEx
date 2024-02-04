namespace FclEx.Serilog;

public static class LoggerExtensions
{
    public static void ActionError(this ILogger logger, string title, Exception ex)
    {
        logger.Error(ex, $"Failed to execute {title} due to {ex.Message}");
    }

    public static ILogger ForContext(this ILogger logger, string name)
    {
        return logger.ForContext(Constants.SourceContextPropertyName, name);
    }
}