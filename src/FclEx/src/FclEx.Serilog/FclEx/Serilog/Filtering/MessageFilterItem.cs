namespace FclEx.Serilog.Filtering;

public record MessageFilterItem(string? Source, string Message, LogEventLevel? MaxLevel) : ILogEventFilterItem
{
    public static implicit operator MessageFilterItem((string? Source, string Message, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.Message, tuple.MaxLevel);
    }

    public static implicit operator MessageFilterItem((string? Source, string Message) tuple)
    {
        return new(tuple.Source, tuple.Message, null);
    }

    public bool Match(LogEvent e)
    {
        return e.MatchMaxLeveOrNull(MaxLevel)
               && e.MatchSourceOrNull(Source)
               && Message.IsNonEmpty()
               && e.MessageTemplate.Text.Contains(Message);
    }

    public static readonly MessageFilterItem[] CommonItems =
    [
        ("DotNetCore.CAP.Processor.TransportCheckProcessor", "Transport connection is unhealthy"),
        ("Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository", "Protected data will be unavailable when container is destroyed"),
        ("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", "No XML encryptor configured")
    ];
}