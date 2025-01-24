namespace FclEx.Serilog;

public record MessageExcluder(string? Source, string Message, LogEventLevel? MaxLevel) : ILogEventExcluder
{
    public static implicit operator MessageExcluder((string? Source, string Message, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.Message, tuple.MaxLevel);
    }

    public static implicit operator MessageExcluder((string? Source, string Message) tuple)
    {
        return new(tuple.Source, tuple.Message, null);
    }

    public bool ShouldExclude(LogEvent e)
    {
        return e.MatchMaxLeveOrNull(MaxLevel)
               && e.MatchSourceOrNull(Source)
               && Message.IsNotEmpty()
               && e.MessageTemplate.Text.Contains(Message);
    }

    public static readonly MessageExcluder[] CommonItems =
    [
        ("DotNetCore.CAP.Processor.TransportCheckProcessor", "Transport connection is unhealthy"),
        ("Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository", "Protected data will be unavailable when container is destroyed"),
        ("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", "No XML encryptor configured"),
    ];
}