using Serilog.Context;

namespace FclEx.Serilog;

public class LogContextProperties : IDisposable
{
    private readonly List<IDisposable> _list = [];

    public LogContextProperties() { }

    public LogContextProperties(string name, object? value, bool destructureObjects = false)
    {
        Push(name, value, destructureObjects);
    }

    public LogContextProperties Push(string name, object? value, bool destructureObjects = false)
    {
        _list.Add(LogContext.PushProperty(name, value, destructureObjects));
        return this;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_list.Count == 0)
            return;

        // NOTE: items are disposed in reverse order because this is actually a scenario first in last out.
        // and we don't use linq.Reverse() here because that will generate new list.
        for (var i = _list.Count - 1; i >= 0; --i)
        {
            var item = _list[i];
            item?.Dispose();
        }

        _list.Clear();
    }
}