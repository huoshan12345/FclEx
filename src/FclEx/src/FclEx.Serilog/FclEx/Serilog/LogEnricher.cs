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

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.TryAddPropIfValid("@service", _service);
        logEvent.TryAddPropIfValid(nameof(EntryAssembly), EntryAssembly);
        logEvent.TryAddPropIfValid(nameof(OsType), OsType);
        logEvent.TryAddPropIfValid(nameof(OsName), OsName);
        logEvent.TryAddPropIfValid(nameof(HostName), HostName);
        logEvent.TryAddPropIfValid(nameof(OsInfo), OsInfo);
    }
}