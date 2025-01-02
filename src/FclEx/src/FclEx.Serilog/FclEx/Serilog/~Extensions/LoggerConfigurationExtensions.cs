namespace FclEx.Serilog;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration WrapAllSinks(this LoggerConfiguration configuration, Func<ILogEventSink, ILogEventSink> wrapSink)
    {
        var sinks = Fields.LoggerConfiguration_Sinks.GetRequiredValue<List<ILogEventSink>>(configuration);

        for (var i = 0; i < sinks.Count; i++)
        {
            sinks[i] = wrapSink(sinks[i]);
        }

        return configuration;
    }
}