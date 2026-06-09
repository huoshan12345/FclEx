namespace FclEx.Web;

public abstract class UserClient<TAccount> : IUserClient<TAccount>, IDisposable where TAccount : IUserAccount
{
    private static int _id;

    private TAccount _account;
    private IHttpService? _httpService;

    protected AsyncLock LoginLocker { get; } = new();
    protected bool _isDisposed;

    public virtual int Id { get; } = Interlocked.Increment(ref _id);

    public virtual IUserClientSession Session { get; } = new UserClientSession();

    public virtual IUserClientState State { get; } = new UserClientState();

    public virtual IHttpService HttpService
    {
        get => _httpService ??= new HttpClientService { Logger = Logger };
        set
        {
            _httpService = value;
            _httpService.Logger = Logger;
        }
    }

    public virtual TAccount Account
    {
        get => _account;
        set
        {
            _account = Check.NotNull(value);
            State.AccountStatus = UserAccountStatus.Normal;
        }
    }

    private Lazy<ILogger> _lazyLogger;

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
                field = null;
                _lazyLogger = new(() => CreateLogger(value));
            }
        }
    }

    protected UserClient(TAccount account, ILoggerFactory? loggerFactory = null)
    {
        _account = Check.NotNull(account);
        _lazyLogger = new(() => CreateLogger(loggerFactory.CreateLoggerOrDefault(GetType())));
    }

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

    protected abstract Task<OperationResult> LoginActionAsync(CancellationToken token);

    protected virtual Task<OperationResult> FakeLoginActionAsync(CancellationToken token)
    {
        return Operation.Success();
    }

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

        using var _ = await LoginLocker.LockAsync(token);

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

    public Task<OperationResult> LoginAsync(CancellationToken token = default)
    {
        return DoLoginAsync(LoginActionWrapperAsync, token);
    }

    public async Task WaitLoginAsync(CancellationToken token = default)
    {
        using (await LoginLocker.LockAsync(token)) { }
    }

    public Task<OperationResult> LogoutAsync(CancellationToken token = default)
    {
        HttpService.ClearAllCookies();
        State.Offline();
        return Operation.Success();
    }

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

    public void Dispose()
    {
        if (_isDisposed)
            return;

        GC.SuppressFinalize(this);
        DisposeAction();
        _isDisposed = true;
    }
}

public abstract class UserClient(IUserAccount? account = null, ILoggerFactory? loggerFactory = null)
    : UserClient<IUserAccount>(account ?? UserAccount.Empty, loggerFactory), IUserClient
{
    protected override ILogger CreateLogger(ILogger logger)
    {
        var propertiesLogger = new PropertiesLogger(logger, GetLogProperties(), GetLogLazyProperties());
        return new UserClientLogger(propertiesLogger, this); // Use non-generic logger for IUserClient
    }
}