namespace FclEx.Web;

/// <summary>
/// Creates user clients from factories registered in an <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Creates a typed-account user client through the registered <see cref="IUserClientFactory{TClient,TAccount}"/>.
    /// </summary>
    /// <typeparam name="TClient">The concrete user-client type.</typeparam>
    /// <typeparam name="TAccount">The account type accepted by the client.</typeparam>
    /// <param name="provider">The service provider that contains the factory registration.</param>
    /// <param name="account">The account assigned to the client.</param>
    /// <param name="httpService">The HTTP service assigned to the client. When <see langword="null"/>, the factory creates one.</param>
    /// <returns>The created client.</returns>
    public static TClient CreateUserClient<TClient, TAccount>(this IServiceProvider provider, TAccount account, IHttpService? httpService = null)
        where TClient : IUserClient<TAccount>
        where TAccount : IUserAccount
    {
        return provider.GetRequiredService<IUserClientFactory<TClient, TAccount>>().Create(account, httpService);
    }

    /// <summary>
    /// Creates a user client through the registered <see cref="IUserClientFactory{TClient}"/>.
    /// </summary>
    /// <typeparam name="TClient">The concrete user-client type.</typeparam>
    /// <param name="provider">The service provider that contains the factory registration.</param>
    /// <param name="account">The account assigned to the client.</param>
    /// <param name="httpService">The HTTP service assigned to the client. When <see langword="null"/>, the factory creates one.</param>
    /// <returns>The created client.</returns>
    public static TClient CreateUserClient<TClient>(this IServiceProvider provider, IUserAccount account, IHttpService? httpService = null)
        where TClient : IUserClient<IUserAccount>
    {
        return provider.CreateUserClient<TClient, IUserAccount>(account, httpService);
    }
}
