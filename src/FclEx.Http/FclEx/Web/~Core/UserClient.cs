using System.Security.Principal;

namespace FclEx.Web;

public abstract class UserClient<TAccount> : IUserClient<TAccount>, IDisposable where TAccount : IUserAccount
{
    private static int _id;

    private readonly Lazy<ILogger> _logger;
    private AccountStatus _accountStatus;
    private TAccount _account;
    private IHttpService? _httpService;

    protected AsyncLock LoginLocker { get; } = new();
    protected bool _isDisposed;

    public virtual int Id { get; } = Interlocked.Increment(ref _id);

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
            AccountStatus = AccountStatus.Normal;
        }
    }
    public virtual AccountStatus AccountStatus
    {
        get => _accountStatus;
        set
        {
            if (_accountStatus == value)
                return;

            _accountStatus = value;
            OnAccountStatusChanged.Invoke(_accountStatus);
        }
    }
    public virtual ILogger Logger => _logger.Value;
    public event Action<AccountStatus> OnAccountStatusChanged = status => { };
    public virtual IUserClientSession Session { get; } = new UserClientSession();
    public virtual bool IsOnline => Session.SessionState == UserClientSessionState.Online;

    protected UserClient(TAccount account, ILoggerFactory? loggerFactory = null)
    {
        _account = Check.NotNull(account);
        _logger = new(() => CreateLogger(loggerFactory), true);
    }

    protected virtual ILogger CreateLogger(ILoggerFactory? factory)
    {
        var logger = factory.Touch().CreateLogger(GetType());
        var logger2 = new PropertiesLogger(logger, GetLogProperties(), GetLogLazyProperties());
        return new UserClientLogger<TAccount>(logger2, this);
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
            (nameof(IsOnline), () => IsOnline),
            (nameof(UserClientSessionState), () => Session.SessionState),
        ];
    }

    protected Task<OperationResult> LoginActionWrapperAsync(CancellationToken token)
    {
        Logger.LogDebug("Start to login...");
        return LoginActionAsync(token)
            .Success(o => Logger.LogDebug("Login successfully"))
            .Error(ex => Logger.LogWarning(ex, "Failed to login: " + ex.Message));
    }

    protected abstract Task<OperationResult> LoginActionAsync(CancellationToken token);

    protected virtual Task<OperationResult> FakeLoginActionAsync(CancellationToken token)
    {
        return Operation.Success().ToTask();
    }

    protected virtual void DisposeAction()
    {
        _httpService?.Dispose();
    }

    protected async Task<OperationResult> DoLoginAsync(Func<CancellationToken, Task<OperationResult>> loginAction, CancellationToken token)
    {
        if (IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operation.Success();
        }

        using var _ = await LoginLocker.LockAsync(token);

        if (IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operation.Success();
        }

        if (token.IsCancellationRequested)
            return Operation.Cancel();

        var time = ValueStopwatch.StartNew();
        try
        {
            Session.LoggingIn();

            var response = await loginAction(token)
                .Success(_ => Session.Online())
                .Error(_ =>
                {
                    if (Session.IsLoggingIn())
                        Session.Offline();
                });

            return response.Elapsed(time.GetElapsedTime());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occured when logging in: " + ex.Message);
            return (ex, time.GetElapsedTime());
        }
        finally
        {
            Logger.LogTrace("It takes {ElapsedSeconds:f3} seconds to login", time.GetElapsedTime().TotalSeconds);
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
        Session.Offline();
        return Operation.Success().ToTask();
    }

    public Task<OperationResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default)
    {
        return DoLoginAsync(async t =>
        {
            Logger.LogDebug("Start to fake login...");
            var result = await FakeLoginActionAsync(t)
                .Success(o => Logger.LogDebug("Fake login successfully"))
                .Error(ex => Logger.LogWarning(ex, "Failed to fake login: " + ex.Message));

            if (result.Error && loginIfFail)
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
    protected override ILogger CreateLogger(ILoggerFactory? factory)
    {
        var logger = factory.Touch().CreateLogger(GetType());
        var logger2 = new PropertiesLogger(logger, GetLogProperties(), GetLogLazyProperties());
        return new UserClientLogger(logger2, this); // Use non-generic logger for IUserClient
    }
}