namespace FclEx.Web;

public static class ServiceCollectionExtensions
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
}