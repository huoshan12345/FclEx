namespace FclEx.Serilog;

public interface  ILogEventExcluder
{
    bool ShouldExclude(LogEvent e);
}