using Newtonsoft.Json.Linq;

namespace FclEx.Serilog.Sinks.Logstash.Inputs;

public readonly struct ErrorInputItem
{
    public ErrorInputItem(JObject item, Exception exception)
    {
        Item = item;
        Exception = exception;
    }

    public JObject Item { get; }
    public Exception Exception { get; }

    public void Deconstruct(out JObject item, out Exception exception)
    {
        item = Item;
        exception = Exception;
    }

    public static implicit operator ErrorInputItem((JObject item, Exception exception) tuple)
    {
        return new ErrorInputItem(tuple.item, tuple.exception);
    }
}