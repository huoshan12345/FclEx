namespace FclEx.Serilog;

public class LogProvider
{
    protected SerilogOptions Options { get; }

    public LogProvider(SerilogOptions options)
    {
        Options = options;
    }

    public virtual ILogger CreateSerilogLogger()
    {
        var configuration = new LoggerConfiguration();
        foreach (var action in Options.ConfigureActions)
        {
            action.Invoke(configuration, Options);
        }
        return configuration.CreateLogger();
    }
}