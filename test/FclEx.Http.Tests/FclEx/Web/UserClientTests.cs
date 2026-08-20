namespace FclEx.Web;

public class UserClientTests : WebTests
{
    [Fact]
    public void Logger_WhenAccountChanges_LogsBeforeAndAfterAccountAssignment()
    {
        var account = new UserAccount("user", "password");
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>());
        client.Logger.LogInformation("test");
        client.Account = account;
        client.Logger.LogInformation("test");
    }

    [Fact]
    public async Task LoginAsync_WhenLoginActionSucceeds_ReturnsSuccess()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>())
        {
            Account = new UserAccount("user", "password"),
        };
        var result = await client.LoginAsync();
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_WhenLoginActionFails_ReturnsFailure()
    {
        var client = new TestUserClient(ServiceProvider.GetRequiredService<ILoggerFactory>(), () => "Login failed")
        {
            Account = new UserAccount("user", "password"),
        };
        var result = await client.LoginAsync();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task LoginAsync_WhenLoginSucceeds_MarksClientOnline()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);

        var result = await client.LoginAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(1, client.LoginCount);
        Assert.True(client.IsOnline);
        Assert.Equal(UserClientSessionStatus.Online, client.State.SessionStatus);
    }

    [Fact]
    public async Task LoginAsync_WhenClientIsAlreadyOnline_DoesNotRunLoginAgain()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);
        client.State.Online();

        var result = await client.LoginAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(0, client.LoginCount);
        Assert.True(client.IsOnline);
    }

    [Fact]
    public async Task LoginAsync_WhenLoginFails_ReturnsOfflineState()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var exception = new InvalidOperationException("invalid credentials");
        var client = new CountingUserClient(loggerFactory)
        {
            LoginResult = exception,
        };

        var result = await client.LoginAsync();

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.Equal(1, client.LoginCount);
        Assert.Equal(UserClientSessionStatus.Offline, client.State.SessionStatus);
    }

    [Fact]
    public async Task LoginAsync_WhenTokenIsCanceledBeforeLogin_ThrowsWithoutRunningLogin()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.LoginAsync(cts.Token));

        Assert.Equal(0, client.LoginCount);
        Assert.Equal(UserClientSessionStatus.Offline, client.State.SessionStatus);
    }

    [Fact]
    public void AccountSetter_UpdatesAccountAndResetsAccountStatusToNormal()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);
        client.State.AccountStatus = UserAccountStatus.Locked;
        var account = new UserAccount("new-user", "new-password");

        client.Account = account;

        Assert.Same(account, client.Account);
        Assert.Equal(UserAccountStatus.Normal, client.State.AccountStatus);
    }

    [Fact]
    public void AccountSetter_WhenAccountIsNull_ThrowsArgumentNullException()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);

        var ex = Assert.Throws<ArgumentNullException>(() => client.Account = null!);

        Assert.Equal("value", ex.ParamName);
    }

    [Fact]
    public void HttpServiceSetter_AssignsLoggerToService()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);
        var service = new TrackingHttpService();

        client.HttpService = service;

        Assert.Same(service, client.HttpService);
        Assert.Same(client.Logger, service.Logger);
    }

    [Fact]
    public void LoggerSetter_WhenLoggerIsNull_UsesUserClientLoggerWrapper()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);

        client.Logger = null!;

        Assert.IsType<UserClientLogger>(client.Logger);
    }

    [Fact]
    public async Task FakeLoginAsync_WhenFakeLoginFailsAndLoginIfFailIsTrue_FallsBackToLogin()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory)
        {
            FakeLoginResult = new InvalidOperationException("fake failed"),
        };

        var result = await client.FakeLoginAsync(loginIfFail: true);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(1, client.FakeLoginCount);
        Assert.Equal(1, client.LoginCount);
        Assert.True(client.IsOnline);
    }

    [Fact]
    public async Task FakeLoginAsync_WhenFakeLoginSucceeds_DoesNotRunRealLogin()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var client = new CountingUserClient(loggerFactory);

        var result = await client.FakeLoginAsync(loginIfFail: true);

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.Equal(1, client.FakeLoginCount);
        Assert.Equal(0, client.LoginCount);
        Assert.True(client.IsOnline);
    }

    [Fact]
    public async Task FakeLoginAsync_WhenFakeLoginFailsAndLoginIfFailIsFalse_ReturnsFailure()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var exception = new InvalidOperationException("fake failed");
        var client = new CountingUserClient(loggerFactory)
        {
            FakeLoginResult = exception,
        };

        var result = await client.FakeLoginAsync(loginIfFail: false);

        Assert.True(result.IsError);
        Assert.Same(exception, result.Exception);
        Assert.Equal(1, client.FakeLoginCount);
        Assert.Equal(0, client.LoginCount);
        Assert.Equal(UserClientSessionStatus.Offline, client.State.SessionStatus);
    }

    [Fact]
    public async Task LogoutAsync_ClearsCookiesAndMarksClientOffline()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new TrackingHttpService();
        var client = new CountingUserClient(loggerFactory)
        {
            HttpService = service,
        };
        client.State.Online();

        var result = await client.LogoutAsync();

        Assert.True(result.IsSuccess, result.Exception?.ToString());
        Assert.True(service.ClearAllCookiesCalled);
        Assert.False(client.IsOnline);
        Assert.Equal(UserClientSessionStatus.Offline, client.State.SessionStatus);
    }

    [Fact]
    public void Dispose_DisposesAssignedHttpServiceOnlyOnce()
    {
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var service = new TrackingHttpService();
        var client = new CountingUserClient(loggerFactory)
        {
            HttpService = service,
        };

        client.Dispose();
        client.Dispose();

        Assert.Equal(1, service.DisposeCount);
    }

    private sealed class CountingUserClient(ILoggerFactory loggerFactory)
        : UserClient(new UserAccount("user", "password"), loggerFactory)
    {
        public OperationResult LoginResult { get; set; } = Operation.Success();

        public OperationResult FakeLoginResult { get; set; } = Operation.Success();

        public int LoginCount { get; private set; }

        public int FakeLoginCount { get; private set; }

        protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
        {
            LoginCount++;
            return LoginResult;
        }

        protected override Task<OperationResult> FakeLoginActionAsync(CancellationToken token)
        {
            FakeLoginCount++;
            return FakeLoginResult;
        }
    }

    private sealed class TrackingHttpService : IHttpService
    {
        private readonly List<Cookie> _cookies =
        [
            new("sid", "abc", "/", "example.com"),
        ];

        public bool ClearAllCookiesCalled => _cookies.All(m => m.Expired);

        public int DisposeCount { get; private set; }

        public Task<FclEx.Http.HttpResponse> SendAsync(FclEx.Http.HttpRequest request, CancellationToken token = default)
        {
            return Task.FromResult(HttpResponse.FromError(request, new NotSupportedException()));
        }

        public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false)
        {
            _cookies.Add(cookie);
        }

        public Cookie? GetCookie(Uri uri, string name) => _cookies.FirstOrDefault(m => m.Name == name);

        public IReadOnlyCollection<Cookie> GetCookies(Uri uri) => _cookies;

        public IReadOnlyCollection<Cookie> GetAllCookies() => _cookies;

        public IWebProxy? Proxy { get; set; }

        public ILogger Logger { get; set; } = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
