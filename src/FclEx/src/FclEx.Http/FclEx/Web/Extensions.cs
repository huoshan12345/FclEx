using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FclEx.Web;

public static class Extensions
{
    public static IServiceCollection AddUserClient<TClient>(this IServiceCollection collection) where TClient : class, IUserClient
    {
        collection.TryAddTransient<TClient>();
        collection.TryAddSingleton(typeof(IUserClientFactory<>), typeof(UserClientFactory<>));
        return collection;
    }
}