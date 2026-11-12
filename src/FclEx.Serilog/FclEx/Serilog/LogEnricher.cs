namespace FclEx.Serilog;

/// <summary>Adds service, entry-assembly, host, and operating-system properties without replacing existing event properties.</summary>
/// <remarks>Static metadata is shared by all instances and read on each event. Configure it before concurrent logging begins.</remarks>
public class LogEnricher : ILogEventEnricher
{
    /// <summary>The entry assembly's simple name, or null when unavailable; emitted as EntryAssembly.</summary>
    public static string? EntryAssembly { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name;
    /// <summary>The OS family returned by GetOs, emitted as OsType.</summary>
    public static string OsType { get; set; } = GetOs();
    /// <summary>The platform identifier from Environment.OSVersion.Platform, emitted as OsName.</summary>
    public static string OsName { get; set; } = Environment.OSVersion.Platform.ToString();
    /// <summary>The machine name, emitted as HostName.</summary>
    public static string HostName { get; set; } = Environment.MachineName;
    /// <summary>The operating-system version description, emitted as OsInfo.</summary>
    public static string OsInfo { get; set; } = Environment.OSVersion.ToString();

    private readonly string? _service;

    /// <summary>Creates an enricher with a fixed Service value and shared host metadata.</summary>
    /// <param name="service">The Service property value, including null when no service name is supplied.</param>
    public LogEnricher(string? service = null)
    {
        _service = service;
    }

    /// <summary>Identifies the current OS family using RuntimeInformation.</summary>
    /// <returns>Linux, Windows, OSX, or FreeBSD on recognized platforms; otherwise RuntimeInformation.OSDescription.</returns>
    public static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return nameof(OSPlatform.Linux);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return nameof(OSPlatform.Windows);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return nameof(OSPlatform.OSX);
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
#if NET5_0_OR_GREATER
            return nameof(OSPlatform.FreeBSD);
#else
            return "FreeBSD";
#endif
        return RuntimeInformation.OSDescription;
    }

    /// <summary>Adds Service, EntryAssembly, OsType, OsName, HostName, and OsInfo if each property is absent.</summary>
    /// <param name="logEvent">The event to enrich in place.</param>
    /// <param name="factory">Creates scalar properties, including null metadata values.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
    {
        logEvent.TryAddProperty(factory, "Service", _service);
        logEvent.TryAddProperty(factory, nameof(EntryAssembly), EntryAssembly);
        logEvent.TryAddProperty(factory, nameof(OsType), OsType);
        logEvent.TryAddProperty(factory, nameof(OsName), OsName);
        logEvent.TryAddProperty(factory, nameof(HostName), HostName);
        logEvent.TryAddProperty(factory, nameof(OsInfo), OsInfo);
    }
}
