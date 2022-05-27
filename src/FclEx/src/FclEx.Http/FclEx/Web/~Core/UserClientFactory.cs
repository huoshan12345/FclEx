using System;
using FclEx.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FclEx.Web
{
    public class UserClientFactory<TClient> : IUserClientFactory<TClient> where TClient : IUserClient
    {
        public UserClientFactory(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public virtual TClient Create(IUserAccount account, IHttpService? httpService = null)
        {
            var client = ServiceProvider.GetRequiredService<TClient>();
            client.Account = account;
            client.HttpService = httpService;
            return client;
        }
    }
}
