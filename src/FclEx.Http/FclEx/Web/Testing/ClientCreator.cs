#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace FclEx.Web.Testing;

public class ClientCreator<TClient, TAccount>(IServiceProvider provider)
    where TClient : IUserClient<TAccount>
    where TAccount : IUserAccount
{
    protected readonly IServiceProvider _provider = provider;

    protected readonly ConcurrentDictionary<TAccount, TClient> _dic = new();

    public virtual string GetCookiesFilePath(IUserAccount account)
    {
        var fileName = $"Cookies_{typeof(TClient).ShortName()}_{account.UserName}.json";
        var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        return path;
    }

    public virtual async Task<IList<SimpleCookie>> ReadCookies(IUserAccount account)
    {
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

    public virtual async Task SaveCookies(TClient client)
    {
        var cookies = client.HttpService.GetAllSimpleCookies();
        var str = cookies.ToJson(new JsonOptions(true));
        var path = GetCookiesFilePath(client.Account);
        await File.WriteAllTextAsync(path, str, Encoding.UTF8);
    }

    public virtual Task<TClient> CreateClient(TAccount account, bool login, bool fakeLogin = true, bool useCache = true, bool readCookie = true, string? proxy = null)
        => CreateClient(account, new LoginOptions(login, fakeLogin, useCache, readCookie, WebProxyHelper.Create(proxy)));

    public virtual async Task<TClient> CreateClient(TAccount account, LoginOptions options)
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
                .OnSucceeded(_ => SaveCookies(client));
        }

        return client;
    }

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

public class ClientCreator<TClient>(IServiceProvider provider) : ClientCreator<TClient, IUserAccount>(provider)
    where TClient : IUserClient
{
    protected static readonly Random Random = new();

    public virtual Task<TClient> CreateRandomClient(int userNameLength, int passwordLength)
    {
        return CreateClient(new UserAccount(Random.NextString(userNameLength), Random.NextString(passwordLength)), false, false, false, false);
    }
}
