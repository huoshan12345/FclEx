namespace FclEx.Web;

/// <summary>
/// Base implementation for stateful user clients.
/// </summary>
/// <typeparam name="TAccount">The account type used by the client.</typeparam>
/// <remarks>
/// Login operations are serialized with an async lock. If the client is already online, login methods return success
/// without invoking the underlying login action. Assigning a new account resets the account status to normal.
/// </remarks>
public abstract class UserClient<TAccount> : IUserClient<TAccount>, IDisposable where TAccount : IUserAccount
{
    private static int _id;

    private TAccount _account;
    private IHttpService? _httpService;

    protected AsyncLock LoginLocker { get; } = new();
    protected bool _isDisposed;

    /// <inheritdoc />
    public virtual int Id { get; } = Interlocked.Increment(ref _id);

    /// <inheritdoc />
    public virtual IUserClientSession Session { get; } = new UserClientSession();

    /// <inheritdoc />
    public virtual IUserClientState State { get; } = new UserClientState();

    /// <inheritdoc />
    public virtual IHttpService HttpService
    {
        get => _httpService ??= new HttpClientService { Logger = Logger };
        set
        {
            _httpService = value;
            _httpService.Logger = Logger;
        }
    }

    /// <inheritdoc />
    public virtual TAccount Account
    {
        get => _account;
        set
        {
            _account = Check.NotNull(value);
            State.AccountStatus = UserAccountStatus.Normal;
        }
    }

    private readonly Lazy<ILogger> _lazyLogger;

    /// <inheritdoc />
    [AllowNull]
    public virtual ILogger Logger
    {
        get => field ?? _lazyLogger.Value;
        set
        {
            if (value is UserClientLogger<TAccount> || value.IsNullOrNullLogger())
            {
                field = value;
            }
            else
            {
                field = CreateLogger(value);
            }

            HttpService.Logger = Logger;
        }
    }

    /// <summary>
    /// Initializes a user client with an account and optional logger factory.
    /// </summary>
    /// <param name="account">The initial account. It cannot be null.</param>
    /// <param name="loggerFactory">Optional factory used to create the underlying logger.</param>
    protected UserClient(TAccount account, ILoggerFactory? loggerFactory = null)
    {
        _account = Check.NotNull(account);
        _lazyLogger = new(() => CreateLogger(loggerFactory.CreateLoggerOrDefault(GetType())));
    }

    /// <summary>
    /// Creates the logger wrapper used by this client.
    /// </summary>
    /// <param name="logger">The inner logger.</param>
    /// <returns>A logger enriched with client/account state.</returns>
    protected virtual ILogger CreateLogger(ILogger logger)
    {
        var propertiesLogger = new PropertiesLogger(logger, GetLogProperties(), GetLogLazyProperties());
        return new UserClientLogger<TAccount>(propertiesLogger, this);
    }

    protected virtual IEnumerable<LoggerProperty> GetLogProperties()
    {
        return
        [
            ("ClientType", GetType().ShortName()),
        ];
    }

    protected virtual IEnumerable<LazyLoggerProperty> GetLogLazyProperties()
    {
        return
        [
            (nameof(Account), () => Account),
            (nameof(State.SessionStatus), () => State.SessionStatus),
            (nameof(State.AccountStatus), () => State.AccountStatus),
        ];
    }

    protected Task<OperationResult> LoginActionWrapperAsync(CancellationToken token)
    {
        Logger.LogDebug("Start to login...");
        return LoginActionAsync(token)
            .OnValue(o => Logger.LogDebug("Login successfully"))
            .OnException(ex => Logger.LogWarning(ex, "Failed to login: {Error}", ex.Message));
    }

    /// <summary>
    /// Runs the implementation-specific real login action.
    /// </summary>
    /// <param name="token">A cancellation token for the login operation.</param>
    /// <returns>The login result.</returns>
    protected abstract Task<OperationResult> LoginActionAsync(CancellationToken token);

    /// <summary>
    /// Runs the implementation-specific fake-login action.
    /// </summary>
    /// <param name="token">A cancellation token for the fake-login operation.</param>
    /// <returns>The fake-login result. The default implementation succeeds without doing work.</returns>
    protected virtual Task<OperationResult> FakeLoginActionAsync(CancellationToken token)
    {
        return Operation.Success();
    }

    /// <summary>
    /// Disposes resources owned by this client.
    /// </summary>
    /// <remarks>The default implementation disposes the assigned HTTP service if one has been created.</remarks>
    protected virtual void DisposeAction()
    {
        _httpService?.Dispose();
    }

    protected async Task<OperationResult> DoLoginAsync(Func<CancellationToken, Task<OperationResult>> loginAction, CancellationToken token)
    {
        if (this.IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operation.Success();
        }

        using var _ = await LoginLocker.AcquireAsync(token);

        if (this.IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operation.Success();
        }

        if (token.IsCancellationRequested)
            return Operation.Cancel();

        var time = ValueStopwatch.StartNew();
        try
        {
            State.LoggingIn();

            var response = await loginAction(token)
                .OnValue(_ => State.Online())
                .OnException(_ =>
                {
                    if (State.IsLoggingIn())
                        State.Offline();
                });

            return response.Elapsed(time.GetElapsedTime());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute login action: {Error}", ex.Message);
            return (ex, time.GetElapsedTime());
        }
        finally
        {
            Logger.LogTrace("It takes {ElapsedSeconds:f3} seconds to execute login action", time.GetElapsedTime().TotalSeconds);
        }
    }

    /// <inheritdoc />
    public Task<OperationResult> LoginAsync(CancellationToken token = default)
    {
        return DoLoginAsync(LoginActionWrapperAsync, token);
    }

    /// <inheritdoc />
    public async Task WaitLoginAsync(CancellationToken token = default)
    {
        using (await LoginLocker.AcquireAsync(token)) { }
    }

    /// <inheritdoc />
    public Task<OperationResult> LogoutAsync(CancellationToken token = default)
    {
        HttpService.ClearAllCookies();
        State.Offline();
        return Operation.Success();
    }

    /// <inheritdoc />
    public Task<OperationResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default)
    {
        return DoLoginAsync(async t =>
        {
            Logger.LogDebug("Start to fake login...");
            var result = await FakeLoginActionAsync(t)
                .OnValue(o => Logger.LogDebug("Fake login successfully"))
                .OnException(ex => Logger.LogWarning(ex, "Failed to fake login: {Error}", ex.Message));

            if (result.IsError && loginIfFail)
            {
                result = await LoginActionWrapperAsync(t);
            }
            return result;
        }, token);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
            return;

        GC.SuppressFinalize(this);
        DisposeAction();
        _isDisposed = true;
    }
}

/// <summary>
/// Base implementation for user clients that use <see cref="IUserAccount"/>.
/// </summary>
public abstract class UserClient(IUserAccount? account = null, ILoggerFactory? loggerFactory = null)
    : UserClient<IUserAccount>(account ?? UserAccount.Empty, loggerFactory), IUserClient
{
    protected override ILogger CreateLogger(ILogger logger)
    {
        var propertiesLogger = new PropertiesLogger(logger, GetLogProperties(), GetLogLazyProperties());
        return new UserClientLogger(propertiesLogger, this); // Use non-generic logger for IUserClient
    }
}
