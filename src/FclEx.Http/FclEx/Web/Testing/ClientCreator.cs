namespace FclEx.Web.Testing;

/// <summary>
/// Creates user-client instances for tests and automation, with optional cookie restore, login, and client caching.
/// </summary>
/// <typeparam name="TClient">The concrete user-client type to create.</typeparam>
/// <typeparam name="TAccount">The account type accepted by the client.</typeparam>
/// <remarks>
/// Clients are created through <see cref="IUserClientFactory{TClient,TAccount}"/> from the supplied service provider.
/// When caching is enabled, clients are cached by the account value in a process-local dictionary. Cookie read/write
/// operations are serialized per client type and account name.
/// </remarks>
public class ClientCreator<TClient, TAccount>(IServiceProvider provider)
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    protected readonly IServiceProvider _provider = provider;

    protected readonly ConcurrentDictionary<TAccount, TClient> _dic = [];
    protected readonly ConcurrentDictionary<string, AsyncLock> _locks = [];

    /// <summary>
    /// Returns the JSON file path used to persist cookies for an account.
    /// </summary>
    /// <param name="account">The account whose cookies are being read or written.</param>
    /// <returns>
    /// A file in the current directory named with the client type short name and account user name.
    /// Override this method to store cookies elsewhere.
    /// </returns>
    public virtual string GetCookiesFilePath(IUserAccount account)
    {
        var fileName = $"Cookies_{typeof(TClient).ShortName()}_{account.UserName}.json";
        var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        return path;
    }

    /// <summary>
    /// Reads previously saved cookies for an account.
    /// </summary>
    /// <param name="account">The account whose cookie file should be read.</param>
    /// <returns>The saved cookies, or an empty list when the cookie file does not exist.</returns>
    /// <remarks>The file content is expected to be JSON previously written by <see cref="SaveCookies"/>.</remarks>
    public virtual async Task<IList<SimpleCookie>> ReadCookies(IUserAccount account)
    {
        using var _ = await GetLock(account).AcquireAsync();

        var path = GetCookiesFilePath(account);
        if (File.Exists(path))
        {
            var str = await File.ReadAllTextAsync(path);
            var cookies = str.FromJson<List<SimpleCookie>>()!;
            return cookies;
        }
        else
        {
            return [];
        }
    }

    protected virtual AsyncLock GetLock(IUserAccount account)
    {
        var key = $"{typeof(TClient).ShortName()}_{account.UserName}";
        return _locks.GetOrAdd(key, _ => new AsyncLock());
    }

    /// <summary>
    /// Saves all cookies currently held by the client's HTTP service.
    /// </summary>
    /// <param name="client">The client whose service cookies should be persisted.</param>
    /// <remarks>Cookies are written as indented JSON to the path returned by <see cref="GetCookiesFilePath"/>.</remarks>
    public virtual async Task SaveCookies(TClient client)
    {
        using var _ = await GetLock(client.Account).AcquireAsync();

        var cookies = client.HttpService.GetAllSimpleCookies();
        var str = cookies.ToJson(new JsonOptions(true));
        var path = GetCookiesFilePath(client.Account);
        await File.WriteAllTextAsync(path, str, Encoding.UTF8);
    }

    /// <summary>
    /// Creates or reuses a client and optionally restores cookies, fake-logs in, and performs real login.
    /// </summary>
    /// <param name="account">The account assigned to the client.</param>
    /// <param name="login">Whether to run real login when the client remains offline after fake login.</param>
    /// <param name="fakeLogin">Whether to run fake login before real login.</param>
    /// <param name="useCache">Whether an existing cached client for the account can be reused.</param>
    /// <param name="readCookie">Whether saved cookies should be loaded into a newly created client.</param>
    /// <param name="proxy">An optional proxy address string for the client's HTTP service.</param>
    /// <param name="cancellation">The cancellation token to cancel the client creation operation.</param>
    /// <returns>The created or cached client.</returns>
    public virtual Task<TClient> CreateClient(
        TAccount account,
        bool login,
        bool fakeLogin = true,
        bool useCache = false,
        bool readCookie = true,
        string? proxy = null,
        CancellationToken cancellation = default)
    {
        return CreateClient(account, new LoginOptions(login, fakeLogin, useCache, readCookie, WebProxy.Create(proxy), cancellation));
    }

    /// <summary>
    /// Creates or reuses a client using the supplied login options.
    /// </summary>
    /// <param name="account">The account assigned to the client.</param>
    /// <param name="options">Client creation, cookie, proxy, and login options.</param>
    /// <returns>The created or cached client.</returns>
    /// <remarks>
    /// If a cached client is already online, no login method is called. Otherwise, fake login runs first when enabled;
    /// real login runs only if the client is still offline and <see cref="LoginOptions.Login"/> is enabled. Cookies are
    /// saved only after a successful real login.
    /// </remarks>
    public virtual async Task<TClient> CreateClient(TAccount account, LoginOptions options)
    {
        if (!options.UseCache || !_dic.TryGetValue(account, out var client))
        {
            client = await CreateClient(account, options.Proxy, options.ReadCookie);
            if (options.UseCache) _dic[account] = client;
        }

        if (!client.IsOnline && options.FakeLogin)
        {
            await client.FakeLoginAsync(false);
        }

        if (!client.IsOnline && options.Login)
        {
            await client.LoginAsync()
                .OnSucceeded(_ => SaveCookies(client));
        }

        return client;
    }

    /// <summary>
    /// Creates a new client instance without applying cache or login options.
    /// </summary>
    /// <param name="account">The account assigned to the client.</param>
    /// <param name="proxy">The proxy used by the client's HTTP service.</param>
    /// <param name="readCookie">Whether saved cookies should be loaded into the new client's service.</param>
    /// <returns>A new client created by the registered user-client factory.</returns>
    public virtual async Task<TClient> CreateClient(TAccount account, IWebProxy? proxy, bool readCookie)
    {
        var factory = _provider.GetRequiredService<IUserClientFactory<TClient, TAccount>>();
        var client = factory.Create(account, HttpClientService.Create(proxy));
        if (readCookie)
        {
            var cookies = await ReadCookies(account);
            client.HttpService.AddCookies(cookies, (string?)null);
        }

        return client;
    }
}

/// <summary>
/// Creates user clients that use the default <see cref="IUserAccount"/> account abstraction.
/// </summary>
/// <typeparam name="TClient">The concrete user-client type to create.</typeparam>
public class ClientCreator<TClient>(IServiceProvider provider) : ClientCreator<TClient, IUserAccount>(provider)
    where TClient : IUserClient
{
    protected static readonly Random Random = new();

    /// <summary>
    /// Creates an offline client with a random username and password.
    /// </summary>
    /// <param name="userNameLength">The random user-name length.</param>
    /// <param name="passwordLength">The random password length.</param>
    /// <returns>A new client created without cache, cookie restore, fake login, or real login.</returns>
    public virtual Task<TClient> CreateRandomClient(int userNameLength, int passwordLength)
    {
        return CreateClient(new UserAccount(Random.NextString(userNameLength), Random.NextString(passwordLength)), false, false, false, false);
    }
}
