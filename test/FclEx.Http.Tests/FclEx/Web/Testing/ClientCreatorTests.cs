namespace FclEx.Web.Testing;

public class ClientCreatorTests
{
    [Fact]
    public void GetCookiesFilePath_UsesClientTypeAndAccountName()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");

        var path = context.Creator.GetCookiesFilePath(account);

        Assert.Equal(
            Path.Combine(context.DirectoryPath, "Cookies_CreatorTestClient_alice.json"),
            path);
    }

    [Fact]
    public async Task ReadCookies_WhenFileDoesNotExist_ReturnsEmptyList()
    {
        using var context = new TestContext();

        var cookies = await context.Creator.ReadCookies(new UserAccount("missing", "pwd"));

        Assert.Empty(cookies);
    }

    [Fact]
    public async Task ReadCookies_WhenFileExists_ReturnsCookiesFromJson()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");
        var expected = new[]
        {
            new SimpleCookie("sid", "abc", "/account", "example.com"),
        };
        await File.WriteAllTextAsync(
            context.Creator.GetCookiesFilePath(account),
            expected.ToJson(new JsonOptions(true)),
            Encoding.UTF8);

        var cookies = await context.Creator.ReadCookies(account);

        var cookie = Assert.Single(cookies);
        Assert.Equal("sid", cookie.Name);
        Assert.Equal("abc", cookie.Value);
        Assert.Equal("/account", cookie.Path);
        Assert.Equal("example.com", cookie.Domain);
    }

    [Fact]
    public async Task CreateClient_WhenUseCacheIsTrue_ReturnsCachedClientForSameAccount()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");
        var options = new LoginOptions(
            Login: false,
            FakeLogin: false,
            UseCache: true,
            ReadCookie: false,
            Proxy: null);

        var first = await context.Creator.CreateClient(account, options);
        var second = await context.Creator.CreateClient(account, options);

        Assert.Same(first, second);
    }

    [Fact]
    public async Task CreateClient_WhenUseCacheIsFalse_CreatesNewClientEachTime()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");
        var options = new LoginOptions(
            Login: false,
            FakeLogin: false,
            UseCache: false,
            ReadCookie: false,
            Proxy: null);

        var first = await context.Creator.CreateClient(account, options);
        var second = await context.Creator.CreateClient(account, options);

        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task CreateClient_WhenReadCookieIsTrue_LoadsCookiesIntoClientService()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");
        var cookies = new[]
        {
            new SimpleCookie("sid", "abc", "/", "example.com"),
        };
        await File.WriteAllTextAsync(
            context.Creator.GetCookiesFilePath(account),
            cookies.ToJson(new JsonOptions(true)),
            Encoding.UTF8);

        var client = await context.Creator.CreateClient(account, proxy: null, readCookie: true);

        var cookie = Assert.Single(client.HttpService.GetAllSimpleCookies());
        Assert.Equal("sid", cookie.Name);
        Assert.Equal("abc", cookie.Value);
        Assert.Equal("/", cookie.Path);
        Assert.Equal("example.com", cookie.Domain);
    }

    [Fact]
    public async Task SaveCookies_WritesClientCookiesToAccountFile()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");
        var client = new CreatorTestClient(account, context.LoggerFactory)
        {
            HttpService = new StoredCookieHttpService([
                new Cookie("sid", "abc", "/", "example.com"),
            ]),
        };

        await context.Creator.SaveCookies(client);

        var json = await File.ReadAllTextAsync(context.Creator.GetCookiesFilePath(account));
        var cookie = Assert.Single(json.FromJson<List<SimpleCookie>>()!);
        Assert.Equal("sid", cookie.Name);
        Assert.Equal("abc", cookie.Value);
        Assert.Equal("/", cookie.Path);
        Assert.Equal("example.com", cookie.Domain);
    }

    [Fact]
    public async Task CreateClient_WhenFakeLoginIsTrueAndClientIsOffline_CallsFakeLogin()
    {
        using var context = new TestContext();

        var client = await context.Creator.CreateClient(
            new UserAccount("alice", "pwd"),
            new LoginOptions(
                Login: false,
                FakeLogin: true,
                UseCache: false,
                ReadCookie: false,
                Proxy: null));

        Assert.Equal(1, client.FakeLoginCount);
        Assert.Equal(0, client.LoginCount);
        Assert.True(client.IsOnline);
    }

    [Fact]
    public async Task CreateClient_WhenLoginIsTrueAndClientIsOffline_CallsLoginAndSavesCookies()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");

        var client = await context.Creator.CreateClient(
            account,
            new LoginOptions(
                Login: true,
                FakeLogin: false,
                UseCache: false,
                ReadCookie: false,
                Proxy: null));

        Assert.Equal(1, client.LoginCount);
        Assert.Equal(0, client.FakeLoginCount);
        Assert.True(client.IsOnline);
        Assert.True(File.Exists(context.Creator.GetCookiesFilePath(account)));
    }

    [Fact]
    public async Task CreateClient_WhenCachedClientIsAlreadyOnline_DoesNotLoginAgain()
    {
        using var context = new TestContext();
        var account = new UserAccount("alice", "pwd");
        var options = new LoginOptions(
            Login: true,
            FakeLogin: true,
            UseCache: true,
            ReadCookie: false,
            Proxy: null);

        var first = await context.Creator.CreateClient(account, options);
        var second = await context.Creator.CreateClient(account, options);

        Assert.Same(first, second);
        Assert.Equal(1, first.FakeLoginCount);
        Assert.Equal(0, first.LoginCount);
    }

    [Fact]
    public async Task CreateClient_WithStringProxy_CreatesClientServiceWithProxy()
    {
        using var context = new TestContext();

        var client = await context.Creator.CreateClient(
            new UserAccount("alice", "pwd"),
            login: false,
            fakeLogin: false,
            useCache: false,
            readCookie: false,
            proxy: "http://127.0.0.1:8888");

        Assert.True(WebProxyInterfaceEqualityComparer.Instance.Equals(
            WebProxyHelper.Create("http://127.0.0.1:8888"),
            client.HttpService.Proxy));
    }

    [Fact]
    public async Task CreateRandomClient_CreatesOfflineClientWithRequestedCredentialLengths()
    {
        using var provider = new ServiceCollection()
            .AddLogging()
            .AddUserClient<TestUserClient>()
            .BuildServiceProvider();
        var creator = new ClientCreator<TestUserClient>(provider);

        var client = await creator.CreateRandomClient(6, 8);

        Assert.Equal(6, client.Account.UserName.Length);
        Assert.Equal(8, client.Account.Password.Length);
        Assert.False(client.IsOnline);
    }

    private sealed class TestContext : IDisposable
    {
        private readonly ServiceProvider _provider;

        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "FclEx.Http.Tests", Guid.NewGuid().ToString("N"));

        public ILoggerFactory LoggerFactory { get; }

        public TestClientCreator Creator { get; }

        public TestContext()
        {
            Directory.CreateDirectory(DirectoryPath);
            _provider = new ServiceCollection()
                .AddLogging()
                .AddUserClient<CreatorTestClient, UserAccount>(UserAccount.Empty)
                .BuildServiceProvider();
            LoggerFactory = _provider.GetRequiredService<ILoggerFactory>();
            Creator = new(_provider, DirectoryPath);
        }

        public void Dispose()
        {
            _provider.Dispose();
            if (Directory.Exists(DirectoryPath))
                Directory.Delete(DirectoryPath, recursive: true);
        }
    }

    private sealed class TestClientCreator(IServiceProvider provider, string directory)
        : ClientCreator<CreatorTestClient, UserAccount>(provider)
    {
        public override string GetCookiesFilePath(IUserAccount account)
        {
            return Path.Combine(directory, $"Cookies_{typeof(CreatorTestClient).ShortName()}_{account.UserName}.json");
        }
    }

    private sealed class CreatorTestClient(UserAccount account, ILoggerFactory loggerFactory)
        : UserClient<UserAccount>(account, loggerFactory)
    {
        public int LoginCount { get; private set; }

        public int FakeLoginCount { get; private set; }

        protected override Task<OperationResult> LoginActionAsync(CancellationToken token)
        {
            LoginCount++;
            return Operation.Success();
        }

        protected override Task<OperationResult> FakeLoginActionAsync(CancellationToken token)
        {
            FakeLoginCount++;
            return Operation.Success();
        }
    }

    private sealed class StoredCookieHttpService(IEnumerable<Cookie> cookies) : IHttpService
    {
        private readonly List<Cookie> _cookies = cookies.ToList();

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
        }
    }
}
