namespace FclEx.Serilog;

public interface  ILogEventFilterItem
{
    bool Match(LogEvent e);
}