namespace FclEx.Web;

/// <summary>
/// Logger wrapper that prefixes log messages with the user client's account name when one is available.
/// </summary>
/// <remarks>
/// For Microsoft.Extensions.Logging formatted states, this type uses reflection to preserve structured values while changing the message template.
/// </remarks>
public class UserClientLogger<TAccount> : ILogger where TAccount : IUserAccount
{
    private readonly ILogger _logger;
    private readonly UserClient<TAccount> _client;

    /// <summary>
    /// Creates a logger wrapper for a user client.
    /// Nested <see cref="UserClientLogger{TAccount}"/> instances are unwrapped so the account prefix is not applied repeatedly.
    /// </summary>
    public UserClientLogger(ILogger logger, UserClient<TAccount> client)
    {
        _logger = logger is UserClientLogger<TAccount> clientLogger
            ? clientLogger._logger
            : logger;
        _client = client;
    }

    private static readonly Type _typeFormattedLogValues = typeof(ILogger).Assembly.GetRequiredType("Microsoft.Extensions.Logging.FormattedLogValues");
    private static readonly FieldInfo _values = _typeFormattedLogValues.GetRequiredField("_values");
    private static readonly FieldInfo _originalMessage = _typeFormattedLogValues.GetRequiredField("_originalMessage");
    private const string NullFormat = "[null]";

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (_client.Account.UserName is not { Length: > 0 } name)
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

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => _logger.BeginScope(state) ?? Disposable.Empty;
}

/// <summary>
/// Non-generic logger wrapper for user clients that expose <see cref="IUserAccount"/>.
/// </summary>
public class UserClientLogger(ILogger logger, UserClient<IUserAccount> client)
    : UserClientLogger<IUserAccount>(logger, client);
