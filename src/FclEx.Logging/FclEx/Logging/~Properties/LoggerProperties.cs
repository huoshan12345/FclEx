namespace FclEx.Logging;

public class LoggerProperties : IDisposable
{
    private readonly List<IDisposable> _list = [];
    private readonly ILogger _logger;

    public LoggerProperties(ILogger logger)
    {
        _logger = Check.NotNull(logger);
    }

    public LoggerProperties(ILogger logger, string name, object? value)
        : this(logger)
    {
        Push(name, value);
    }


    public LoggerProperties Push(string name, object? value)
    {
        Check.NotEmpty(name);

        _list.Add(_logger.PushProperty(name, value));
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