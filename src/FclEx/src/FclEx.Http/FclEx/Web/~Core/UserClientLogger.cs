namespace FclEx.Web;

public class UserClientLogger : ILogger
{
    private readonly ILogger _logger;
    private readonly UserClient _client;

    public UserClientLogger(ILogger logger, UserClient client)
    {
        _logger = logger;
        _client = client;
    }

    private static readonly Type _typeFormattedLogValues = typeof(ILogger).Assembly.GetType("Microsoft.Extensions.Logging.FormattedLogValues") ?? throw new ArgumentNullException(nameof(_typeFormattedLogValues));
    private static readonly FieldInfo _values = _typeFormattedLogValues.GetRequiredField("_values");
    private static readonly FieldInfo _originalMessage = _typeFormattedLogValues.GetRequiredField("_originalMessage");
    private const string NullFormat = "[null]";

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_client.Account?.UserName is not { Length: > 0 } name)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        if (typeof(TState) == _typeFormattedLogValues)
        {
            var values = _values.GetValue<object?[]?>(state);
            var originalMessage = _originalMessage.GetRequiredValue<string>(state);
            var format = originalMessage == NullFormat ? null : originalMessage;
            var newFormat = StringBuilderHelper.Build(m => AppendAccountName(m, name).Append(format));
            var newState = _typeFormattedLogValues.CreateObject(newFormat, values).CastTo<TState>();
            _logger.Log(logLevel, eventId, newState, exception, formatter);
        }
        else
        {
            _logger.Log(logLevel, eventId, state, exception, (s, ex) => $"[{name}]{formatter(s, ex)}");
        }
    }

    private static StringBuilder AppendAccountName(StringBuilder builder, string name)
    {
        builder.Append('[');
        builder.Append(name);
        builder.Append(']');
        return builder;
    }

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state) ?? EmptyDisposable.Instance;
}