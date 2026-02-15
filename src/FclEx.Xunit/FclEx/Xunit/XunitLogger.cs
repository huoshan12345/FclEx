using FclEx.Helpers;

namespace FclEx.Xunit;

public class XunitLogger : ILogger
{
    private readonly string _name;
    private readonly Func<ITestOutputHelper?> _outputResolver;
    private readonly bool _consoleFallback;

    private const string CheckDisposedFieldName =
#if FCLEX_XUNIT_V3
        "state";
#else
        "buffer";
#endif

    private static readonly FieldInfo? _fieldToCheckDisposed = typeof(TestOutputHelper).GetField(CheckDisposedFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

    public XunitLogger(string name, Func<ITestOutputHelper?> outputResolver, bool consoleFallback)
    {
        _outputResolver = outputResolver;
        _consoleFallback = consoleFallback;
        _name = name;
    }

    private static bool CheckDisposed(ITestOutputHelper output)
    {
        if (output is TestOutputHelper helper && _fieldToCheckDisposed is not null)
        {
            return _fieldToCheckDisposed.GetValue(helper) == null;
        }
        else
        {
            return false;
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
    {
        if (!IsEnabled(logLevel) || formatter == null)
            return;

        var output = _outputResolver();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (output is null || CheckDisposed(output))
        {
            if (_consoleFallback == false)
                return;

            output = null;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        using var x = StringBuilderHelper.GetCached();
        var builder = x.Value;

        builder.AppendSquareBracketed(_name);
        builder.AppendSquareBracketed(logLevel.ToString());
        builder.Append(message);

        if (exception is not null)
        {
            var ex = exception.ToString();
            if (message.Contains(ex) == false)
            {
                // Append exception details if not already included in the message
                builder.Append(Environment.NewLine);
                builder.Append(ex);
            }
        }

        var str = builder.ToString();

        if (output is null)
        {
            Console.WriteLine(str);
        }
        else
        {
            output.WriteLine(str);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => Disposable.Empty;
}