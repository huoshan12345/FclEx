namespace FclEx.Serilog;

/// <summary>Excludes events containing a case-sensitive substring in their raw message-template text.</summary>
/// <param name="Source">The required source or dot-separated ancestor, or null for any source.</param>
/// <param name="Message">The nonempty substring to find in MessageTemplate.Text; rendered property values are not searched.</param>
/// <param name="MaxLevel">An inclusive maximum level, or null for no level restriction.</param>
public record MessageExcluder(string? Source, string Message, LogEventLevel? MaxLevel) : ILogEventExcluder
{
    /// <summary>Creates a rule from source, message substring, and inclusive maximum level.</summary>
    /// <param name="tuple">The rule settings.</param>
    /// <returns>A rule with those settings.</returns>
    public static implicit operator MessageExcluder((string? Source, string Message, LogEventLevel? MaxLevel) tuple)
    {
        return new(tuple.Source, tuple.Message, tuple.MaxLevel);
    }

    /// <summary>Creates a message rule with no level restriction.</summary>
    /// <param name="tuple">The source restriction and template substring.</param>
    /// <returns>A rule whose MaxLevel is null.</returns>
    public static implicit operator MessageExcluder((string? Source, string Message) tuple)
    {
        return new(tuple.Source, tuple.Message, null);
    }

    /// <summary>Checks the maximum level, source, and raw-template substring.</summary>
    /// <param name="e">The event to inspect.</param>
    /// <returns>True when every restriction matches. An empty Message never excludes an event.</returns>
    public bool ShouldExclude(LogEvent e)
    {
        return e.MatchMaxLevelOrNull(MaxLevel)
               && e.MatchSourceOrNull(Source)
               && Message.IsNotEmpty()
               && e.MessageTemplate.Text.Contains(Message);
    }

    /// <summary>Built-in CAP transport and ASP.NET Core data-protection message exclusions, with no maximum level restriction.</summary>
    public static readonly MessageExcluder[] CommonItems =
    [
        ("DotNetCore.CAP.Processor.TransportCheckProcessor", "Transport connection is unhealthy"),
        ("Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository", "Protected data will be unavailable when container is destroyed"),
        ("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", "No XML encryptor configured"),
    ];
}
