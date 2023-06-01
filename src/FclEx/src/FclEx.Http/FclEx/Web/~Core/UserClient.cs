using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http;
using FclEx.Utils;
using Microsoft.Extensions.Logging.Abstractions;
using Nito.AsyncEx;

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
    public virtual event Action<AccountStatus> OnAccountStatusChanged = status => { };
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
        return new LazyLoggerProperty[]
        {
            (nameof(Account), () => Account),
            (nameof(IsOnline), () => IsOnline),
            (nameof(SessionState), () => Session.State)
        };
    }

    protected Task<OperateResult> DoLoginInternal(CancellationToken token)
    {
        Logger.LogDebug("Start to login...");
        return LoginInternal(token)
            .Ok(o => Logger.LogDebug("Login successfully"))
            .Error(ex => Logger.LogWarning(ex, "Failed to login: " + ex.Message));
    }

    protected abstract Task<OperateResult> LoginInternal(CancellationToken token);

    protected virtual Task<OperateResult> FakeLoginInternal(CancellationToken token)
    {
        return Operate.Success.ToTask();
    }

    protected virtual void DisposeInternal()
    {
        _httpService?.Dispose();
    }

    protected async Task<OperateResult> DoLoginAction(Func<CancellationToken, Task<OperateResult>> loginAction, CancellationToken token)
    {
        if (IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operate.Success;
        }

        using var _ = await LoginLocker.LockAsync(token);

        if (IsOnline)
        {
            Logger.LogTrace("Already online");
            return Operate.Success;
        }

        if (token.IsCancellationRequested)
            return Operate.Cancel;

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
                .DonotCapture();
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

    public Task<OperateResult> Login(CancellationToken token = default)
    {
        return DoLoginAction(DoLoginInternal, token);
    }

    public async Task WaitForLogin(CancellationToken token = default)
    {
        using (await LoginLocker.LockAsync(token)) { }
    }

    public Task<OperateResult> Logout(CancellationToken token = default)
    {
        HttpService.ClearAllCookies();
        Session.Offline();
        Session.LoginCaptcha = null;
        Session.LoginCaptchaBytes = null;
        return Operate.Success.ToTask();
    }

    public Task<OperateResult> FakeLogin(bool loginIfFail = true, CancellationToken token = default)
    {
        return DoLoginAction(async t =>
        {
            Logger.LogTrace("Start to fake login...");
            var result = await FakeLoginInternal(t)
                .Ok(o => Logger.LogTrace("Fake login successfully"))
                .Error(ex => Logger.LogWarning(ex, "Failed to fake login : " + ex.Message))
                .DonotCapture();

            if (result.Error && loginIfFail)
            {
                result = await DoLoginInternal(t).DonotCapture();
            }
            return result;
        }, token);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        DisposeInternal();
    }
}