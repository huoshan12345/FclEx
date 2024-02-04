using System.Reflection;
using System.Runtime.InteropServices;

namespace FclEx.Serilog;

public class LogEnricher : ILogEventEnricher
{
    public static string? EntryAssembly { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name;
    public static string OsType { get; set; } = GetOs();
    public static string OsName { get; set; } = Environment.OSVersion.Platform.ToString();
    public static string HostName { get; set; } = Environment.MachineName;
    public static string OsInfo { get; set; } = Environment.OSVersion.ToString();

    private readonly string? _service;

    public LogEnricher(string? service = null)
    {
        _service = service;
    }

    public static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return nameof(OSPlatform.Linux);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return nameof(OSPlatform.Windows);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return nameof(OSPlatform.OSX);
        return RuntimeInformation.OSDescription;
    }

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