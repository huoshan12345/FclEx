using FclEx.Web.Core;
using FclEx.Web.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FclEx.Web
{
    public static class Extensions
    {
        public static IServiceCollection AddUserClient<TClient, TAccount>(this IServiceCollection collection)
            where TClient : class, IUserClient, IHasAccount<TAccount>
        {
            collection.TryAddTransient<TClient>();
            collection.TryAddSingleton<IUserClientFactory<TClient, TAccount>, UserClientFactory<TClient, TAccount>>();
            return collection;
        }

        public static IServiceCollection AddUserClient<TClient>(this IServiceCollection collection)
            where TClient : class, IUserClient, IHasAccount<UserAccount>
        {
            return collection.AddUserClient<TClient, UserAccount>();
        }
    }
}
