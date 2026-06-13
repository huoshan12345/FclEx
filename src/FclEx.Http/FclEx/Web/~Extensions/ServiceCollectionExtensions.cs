namespace FclEx.Web;

/// <summary>
/// Registers user-client factories and default account instances.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a typed user client and factory for a custom account type.
    /// </summary>
    /// <typeparam name="TClient">The concrete user-client type.</typeparam>
    /// <typeparam name="TAccount">The account type accepted by the client.</typeparam>
    /// <param name="collection">The service collection to add registrations to.</param>
    /// <param name="emptyAccount">The account instance registered as the default <typeparamref name="TAccount"/>.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>Existing registrations are preserved because this method uses <c>TryAdd*</c> registrations.</remarks>
    public static IServiceCollection AddUserClient<TClient, TAccount>(this IServiceCollection collection, TAccount emptyAccount) where TClient : class, IUserClient<TAccount> where TAccount : class, IUserAccount
    {
        collection.TryAddTransient<TClient>();
        collection.TryAddSingleton<TAccount>(emptyAccount);
        collection.TryAddSingleton(typeof(IUserClientFactory<,>), typeof(UserClientFactory<,>));
        return collection;
    }

    /// <summary>
    /// Registers a user client that uses <see cref="IUserAccount"/>.
    /// </summary>
    /// <typeparam name="TClient">The concrete user-client type.</typeparam>
    /// <param name="collection">The service collection to add registrations to.</param>
    /// <returns>The same service collection.</returns>
    /// <remarks>
    /// Registers <see cref="UserAccount.Empty"/> as both <see cref="IUserAccount"/> and <see cref="UserAccount"/>.
    /// Existing registrations are preserved because this method uses <c>TryAdd*</c> registrations.
    /// </remarks>
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
