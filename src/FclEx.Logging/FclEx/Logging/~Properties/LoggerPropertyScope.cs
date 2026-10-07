namespace FclEx.Logging;

/// <summary>Accumulates structured property scopes and releases them together in reverse order.</summary>
/// <remarks>
/// Each push begins a scope immediately in the logger's current context. Dispose this object before its
/// enclosing scope, and dispose nested scopes in reverse order. Values are retained by reference.
/// This object does not synchronize concurrent pushes and disposal.
/// </remarks>
public class LoggerPropertyScope : IDisposable
{
    private readonly List<IDisposable> _scopes = [];
    private readonly ILogger _logger;

    /// <summary>Creates an empty scope group; no logger scope is started until a property is pushed.</summary>
    /// <param name="logger">The logger on which property scopes are started.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public LoggerPropertyScope(ILogger logger)
    {
        _logger = Check.NotNull(logger);
    }

    /// <summary>Creates a scope group and immediately pushes its first property.</summary>
    /// <param name="logger">The logger on which property scopes are started.</param>
    /// <param name="name">The nonempty structured property name.</param>
    /// <param name="value">The property value, which may be null.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public LoggerPropertyScope(ILogger logger, string name, object? value)
        : this(logger)
    {
        Push(name, value);
    }

    /// <summary>Immediately begins a scope containing a property and adds its handle to this group.</summary>
    /// <param name="name">The nonempty structured property name.</param>
    /// <param name="value">The property value, which may be null.</param>
    /// <returns>This scope group so that more properties can be pushed.</returns>
    /// <remarks>Duplicate names are preserved in nested scopes; the provider determines their precedence.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public LoggerPropertyScope Push(string name, object? value)
    {
        Check.NotEmpty(name);

        _scopes.Add(_logger.PushProperty(name, value));
        return this;
    }

    /// <summary>Disposes accumulated scope handles in reverse push order and clears the group.</summary>
    /// <remarks>Repeated calls do nothing unless additional properties have been pushed.</remarks>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_scopes.Count == 0)
            return;

        for (var i = _scopes.Count - 1; i >= 0; --i)
        {
            _scopes[i].Dispose();
        }

        _scopes.Clear();
    }
}
