namespace FclEx.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddCollecting(this ILoggingBuilder builder)
    {
        builder.Services.AddCollecting();
        return builder;
    }
}
