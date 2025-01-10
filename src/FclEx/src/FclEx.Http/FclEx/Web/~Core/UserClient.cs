using FclEx.Logging;

namespace FclEx.Web;

public abstract class UserClient : IUserClient, IDisposable
{
    private static int _id;

    private readonly Lazy<ILogger> _logger;
    private AccountStatus _accountStatus;
    private IUserAccount? _account;
    private IHttpService? _httpService;

    protected AsyncLock LoginLocker { get; } = new();
    protected bool _isDisposed;

    public virtual int Id { get; } = Interlocked.Increment(ref _id);

    [AllowNull]
    public virtual IHttpService HttpService
    {
        get => _httpService ??= new HttpClientService { Logger = Logger };
        set
        {
            if (value == null)
                return;

            _httpService = value;
            _httpService.Logger = Logger;
        }
    }

    [AllowNull]
    public virtual IUserAccount Account
    {
        get => _account ??= new UserAccount();
        set
        {
            if (value == null)
                return;

            _account = value;
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
    public virtual ISession Session { get; } = new Session();
    public virtual bool IsOnline => Session.State == SessionState.Online;

    protected UserClient(IUserAccount? account = null, ILoggerFactory? loggerFactory = null)
    {
        _account = account;
        _logger = new(() => CreateLogger(loggerFactory), true);
    }

    protected virtual ILogger CreateLogger(ILoggerFactory? factory)
    {
        var logger = factory.Touch().CreateLogger(GetType());
        var logger2 = new PropertiesLogger(logger, GetLogProperties(), GetLogLazyProperties());
        return new UserClientLogger(logger2, this);
    }

    protected virtual IEnumerable<LoggerProperty> GetLogProperties()
    {
        return new LoggerProperty[]
        {
            ("ClientType", GetType().ShortName()),
        };
    }

    protected virtual IEnumerable<LazyLoggerProperty> GetLogLazyProperties()
    {
        return
        [
            (nameof(Account), () => Account),
            (nameof(IsOnline), () => IsOnline),
            (nameof(SessionState), () => Session.State),
        ];
    }

    protected Task<OperationResult> LoginActionWrapperAsync(CancellationToken token)
    {
        Logger.LogDebug("Start to login...");
        return LoginActionAsync(token)
            .Ok(o => Logger.LogDebug("Login successfully"))
            .Error(ex => Logger.LogWarning(ex, "Failed to login: " + ex.Message));
    }

    protected abstract Task<OperationResult> LoginActionAsync(CancellationToken token);

    protected virtual Task<OperationResult> FakeLoginActionAsync(CancellationToken token)
    {
        return Operation.Success.ToTask();
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
            return Operation.Success;
        }

        using var _ = await LoginLocker.LockAsync(token);

        if (IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operation.Success;
        }

        if (token.IsCancellationRequested)
            return Operation.Cancel;

        var time = ValueStopwatch.StartNew();
        try
        {
            Session.Logining();
            var res = await loginAction(token)
                .Ok(_ => Session.Online())
                .Error(_ =>
                {
                    if (Session.IsLogining())
                        Session.Offline();
                })
                .IgnoreSyncContext();
            return res.Elapsed(time.GetElapsedTime());
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occured when logging in: " + ex.Message);
            return (ex, time.GetElapsedTime());
        }
        finally
        {
            if (Logger.IsEnabled(LogLevel.Trace))
                Logger.LogTrace($"It takes {time.GetElapsedTime().TotalSeconds:f3} seconds to login");
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
        Session.LoginCaptcha = null;
        Session.LoginCaptchaBytes = null;
        return Operation.Success.ToTask();
    }

    public Task<OperationResult> FakeLoginAsync(bool loginIfFail = true, CancellationToken token = default)
    {
        return DoLoginAsync(async t =>
        {
            Logger.LogTrace("Start to fake login...");
            var result = await FakeLoginActionAsync(t)
                .Ok(o => Logger.LogTrace("Fake login successfully"))
                .Error(ex => Logger.LogWarning(ex, "Failed to fake login: " + ex.Message))
                .IgnoreSyncContext();

            if (result.Error && loginIfFail)
            {
                result = await LoginActionWrapperAsync(t).IgnoreSyncContext();
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