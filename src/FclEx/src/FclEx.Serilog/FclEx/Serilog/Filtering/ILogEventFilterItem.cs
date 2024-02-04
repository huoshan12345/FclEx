namespace FclEx.Serilog.Filtering;

public interface  ILogEventFilterItem
{
    bool Match(LogEvent e);
}