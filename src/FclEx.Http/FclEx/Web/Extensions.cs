namespace FclEx.Web;

public static class Extensions
{
    public static IServiceCollection AddUserClient<TClient, TAccount>(this IServiceCollection collection, TAccount emptyAccount) where TClient : class, IUserClient<TAccount> where TAccount : class, IUserAccount
    {
        collection.TryAddTransient<TClient>();
        collection.TryAddSingleton<TAccount>(emptyAccount);
        collection.TryAddSingleton(typeof(IUserClientFactory<,>), typeof(UserClientFactory<,>));
        return collection;
    }

    public static IServiceCollection AddUserClient<TClient>(this IServiceCollection collection) where TClient : class, IUserClient
    {
        collection.TryAddTransient<TClient>();
        collection.TryAddSingleton<IUserAccount>(UserAccount.Empty);
        collection.TryAddSingleton<UserAccount>(UserAccount.Empty);
        collection.TryAddSingleton(typeof(IUserClientFactory<,>), typeof(UserClientFactory<,>));
        collection.TryAddSingleton(typeof(IUserClientFactory<>), typeof(UserClientFactory<>));
        return collection;
    }

    public static TClient CreateUserClient<TClient, TAccount>(this IServiceProvider provider, TAccount account, IHttpService? httpService = null)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return provider.GetRequiredService<IUserClientFactory<TClient, TAccount>>().Create(account, httpService);
    }

    public static TClient CreateUserClient<TClient>(this IServiceProvider provider, IUserAccount account, IHttpService? httpService = null)
        where TClient : IUserClient<IUserAccount>
    {
        return provider.CreateUserClient<TClient, IUserAccount>(account, httpService);
    }
}