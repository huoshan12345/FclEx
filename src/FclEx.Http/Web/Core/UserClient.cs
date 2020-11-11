using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Services;
using FclEx.Utils;
using FclEx.Web.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq;
using Nito.AsyncEx;

namespace FclEx.Web.Core
{
    public abstract class UserClient : IUserClient, IDisposable
    {
        private static int _id;
        private IHttpService? _httpService;
        protected AsyncLock LockerOfLogin { get; } = new AsyncLock();
        protected bool _isDisposed;

        public int Id { get; } = Interlocked.Increment(ref _id);
        public abstract bool IsOnline { get; }
        public abstract ILogger Logger { get; }
        [AllowNull]
        public IHttpService HttpService
        {
            get => _httpService ??= new HttpClientService { Logger = Logger };
            set
            {
                if (value != null)
                {
                    _httpService = value;
                    _httpService.Logger = Logger;
                }
            }
        }
        public abstract Task<OperateResult> Login(CancellationToken token = default);
        public abstract Task<OperateResult> FakeLogin(bool appendLoginIfFail = true, CancellationToken token = default);
        public abstract Task<OperateResult> Logout(CancellationToken token = default);
        public abstract Task WaitForLogin(CancellationToken token = default);

        protected virtual void DisposeInternal()
        {
            _httpService?.Dispose();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                DisposeInternal();
                _isDisposed = true;
            }
        }
    }

    public abstract class UserClient<TAccount> : UserClient, IHasAccount<TAccount>
    {
        private readonly Lazy<ILogger> _logger;
        private AccountStatus _accountStatus;
        private TAccount _account = default!;

        [DisallowNull, MaybeNull]
        public TAccount Account
        {
            get => _account;
            set
            {
                if (EqualityComparer<TAccount>.Default.Equals(_account, value))
                    return;

                _account = value;
                AccountStatus = AccountStatus.Normal;
            }
        }

        public AccountStatus AccountStatus
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
        public event Action<AccountStatus> OnAccountStatusChanged = status => { };
        public override ILogger Logger => _logger.Value;

        protected UserClient(ILoggerFactory? loggerFactory)
        {
            var innerLogger = loggerFactory == null
                ? NullLogger.Instance
                : loggerFactory.CreateLogger(GetType());
            _logger = new Lazy<ILogger>(() => new PropertiesLogger(innerLogger, GetLogProperties(), GetLogLazyProperties()), true);
        }

        protected virtual IEnumerable<(string, object)> GetLogProperties()
        {
            return new[] { ("ClientType", (object)GetType().ShortName()) };
        }

        protected virtual IEnumerable<(string, Func<object>)> GetLogLazyProperties()
        {
            return new (string, Func<object>)[]
            {
                (nameof(Account), () => Account!),
                (nameof(IsOnline), () => IsOnline),
            };
        }
    }

    public abstract class UserClient<TAccount, TSession> : UserClient<TAccount>, IHasSession, IHasSession<TSession>
        where TSession : class, ISession, new()
    {
        public TSession Session { get; } = new TSession();
        public override bool IsOnline => Session.State == SessionState.Online;
        ISession IHasSession.Session => Session;

        protected UserClient(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        protected override IEnumerable<(string, Func<object>)> GetLogLazyProperties()
        {
            return base.GetLogLazyProperties()
                .Append((nameof(SessionState), () => Session.State));
        }

        protected async Task<OperateResult> DoLoginAction(Func<CancellationToken, Task<OperateResult>> loginAction, CancellationToken token)
        {
            if (IsOnline)
            {
                Logger.LogTrace("Already online");
                return OperateResult.Success;
            }

            using (await LockerOfLogin.LockAsync(token))
            {
                if (IsOnline)
                {
                    Logger.LogTrace("Already online");
                    return OperateResult.Success;
                }

                if (token.IsCancellationRequested)
                    return OperateResult.Cancel;

                try
                {
                    Session.State = SessionState.Logining;
                    var res = await loginAction(token)
                        .Ok(_ => Session.Online())
                        .DonotCapture();
                    return res;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "An error occured when logging in: " + ex.Message);
                    return ex;
                }
            }
        }

        public override Task<OperateResult> Login(CancellationToken token = default)
        {
            return DoLoginAction(DoLoginInternal, token);
        }

        protected Task<OperateResult> DoLoginInternal(CancellationToken token)
        {
            Logger.LogDebug("Start to login...");
            return LoginInternal(token)
                .Ok(o => Logger.LogDebug("Login successfully"))
                .Error(ex => Logger.LogWarning(ex, "Failed to login: " + ex.Message));
        }

        protected abstract Task<OperateResult> LoginInternal(CancellationToken token);

        public override async Task WaitForLogin(CancellationToken token = default)
        {
            using (await LockerOfLogin.LockAsync(token)) { }
        }

        public override Task<OperateResult> Logout(CancellationToken token = default)
        {
            HttpService.ClearAllCookies();
            Session.Offline();
            Session.LoginCaptcha = null;
            Session.LoginCaptchaBytes = null;
            return OperateResult.Success.ToTask();
        }

        public override Task<OperateResult> FakeLogin(bool appendLoginIfFail = true, CancellationToken token = default)
        {
            return DoLoginAction(async t =>
            {
                Logger.LogTrace("Start to fake login...");
                var result = await FakeLoginInternal(t)
                    .Ok(o => Logger.LogTrace("Fake login successfully"))
                    .Error(ex => Logger.LogWarning(ex, "Failed to fake login : " + ex.Message))
                    .DonotCapture();

                if (result.HasError() && appendLoginIfFail)
                {
                    result = await DoLoginInternal(t).DonotCapture();
                }
                return result;
            }, token);
        }

        protected virtual Task<OperateResult> FakeLoginInternal(CancellationToken token)
        {
            return OperateResult.Success.ToTask();
        }
    }
}
