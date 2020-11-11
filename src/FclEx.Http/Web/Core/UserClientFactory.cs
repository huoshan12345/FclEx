using System;
using System.Diagnostics.CodeAnalysis;
using FclEx.Http.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FclEx.Web.Core
{
    public class UserClientFactory<TClient, TAccount> : IUserClientFactory<TClient, TAccount>
        where TClient : IUserClient, IHasAccount<TAccount>
    {
        public UserClientFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public virtual TClient Create([DisallowNull] TAccount account, IHttpService? httpService = null)
        {
            var client = ServiceProvider.GetRequiredService<TClient>();
            client.Account = account;
            client.HttpService = httpService;
            return client;
        }
    }
}
