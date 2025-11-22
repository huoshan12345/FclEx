using Xunit.v3;

namespace FclEx.Xunit;

public class TestLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    private readonly string _name;
    private readonly bool _checkDisposed;
    private bool _isDisposed;
    private readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
        _lock = new();

    private static readonly FieldInfo? _field = typeof(TestOutputHelper).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance);

    public TestLogger(ITestOutputHelper output, string name, bool checkDisposed)
    {
        _output = output;
        _name = name;
        _checkDisposed = checkDisposed;
    }

    private static bool CheckDisposed(ITestOutputHelper output)
    {
        if (output is TestOutputHelper helper && _field is not null)
        {
            return _field.GetValue(helper) == null;
        }
        else
        {
            return false;
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
    {
        if (!IsEnabled(logLevel) || formatter == null || _isDisposed)
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        if (_checkDisposed)
        {
            if (_isDisposed)
                return;

            lock (_lock)
            {
                if (_isDisposed)
                    return;

                if (CheckDisposed(_output))
                {
                    _isDisposed = true;
                    return;
                }
            }
        }

        if (exception is not null)
        {
            var ex = exception.ToString();
            if (message.Contains(ex) == false)
            {
                // Append exception details if not already included in the message
                message = message + Environment.NewLine + ex;
            }
        }

        _output.WriteLine($"[{_name}][{logLevel}]" + message);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => Disposable.Empty;
}