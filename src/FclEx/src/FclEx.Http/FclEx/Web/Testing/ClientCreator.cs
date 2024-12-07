using FclEx.Json;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace FclEx.Web.Testing;

public class ClientCreator<TClient> where TClient : IUserClient
{
    protected readonly IServiceProvider _provider;

    protected readonly ConcurrentDictionary<UserAccount, TClient> _dic = new(UserAccountEqualityComparer.Instance);

    public ClientCreator(IServiceProvider provider)
    {
        _provider = provider;
    }

    public virtual (string Path, bool Exist) GetCookiesFilePath(Type clientType, IUserAccount account)
    {
        var fileName = $"Cookies_{clientType.ShortName()}_{account.UserName}.json";
        var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        return (path, File.Exists(path));
    }

    public virtual async Task<IList<SimpleCookie>> ReadCookies(Type clientType, UserAccount account)
    {
        var (path, exist) = GetCookiesFilePath(clientType, account);
        if (exist)
        {
#if NETSTANDARD2_0
            var str = File.ReadAllText(path);
#else            
            var str = await File.ReadAllTextAsync(path);
#endif
            var cookies = str.FromJson<List<SimpleCookie>>()!;
            return cookies;
        }
        else
        {
            return Array.Empty<SimpleCookie>();
        }
    }

    public virtual async Task SaveCookies<T>(T client) where T : IUserClient
    {
        var cookies = client.HttpService.GetAllSimpleCookies();
        var str = cookies.ToJson(new JsonOptions(true));
        var (path, exist) = GetCookiesFilePath(client.GetType(), client.Account);
#if NETSTANDARD2_0
        File.WriteAllText(path, str, Encoding.UTF8);
#else
        await File.WriteAllTextAsync(path, str, Encoding.UTF8);
#endif
    }

    public virtual Task<TClient> CreateClient(UserAccount account, bool login, bool fakeLogin = true, bool useCache = true, bool readCookie = true, string? proxy = null)
        => CreateClient(account, new LoginOptions(login, fakeLogin, useCache, readCookie, WebProxyHelper.Create(proxy)));

    public virtual async Task<TClient> CreateClient(UserAccount account, LoginOptions options)
    {
        if (!options.UseCache || !_dic.TryGetValue(account, out var client))
        {
            client = await CreateClient(account, options.Proxy, options.ReadCookie);
            if (options.UseCache) _dic[account] = client;
        }

        if (!client.IsOnline && options.FakeLogin)
        {
            if (options.FakeLogin)
            {
                await client.FakeLoginAsync(false);
            }
        }

        if (!client.IsOnline && options.Login)
        {
            await client.LoginAsync()
                .Ok(_ => SaveCookies(client));
        }

        return client;
    }

    public virtual async Task<TClient> CreateClient(UserAccount account, IWebProxy? proxy, bool readCookie)
    {
        var factory = _provider.GetRequiredService<IUserClientFactory<TClient>>();
        var client = factory.Create(account, HttpClientService.Create(proxy));
        if (readCookie)
        {
            var cookies = await ReadCookies(typeof(TClient), account);
            client.HttpService.AddCookies(cookies, (string?)null);
        }

        return client;
    }
}