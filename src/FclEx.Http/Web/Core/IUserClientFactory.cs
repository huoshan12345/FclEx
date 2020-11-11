using System;
using System.Diagnostics.CodeAnalysis;
using FclEx.Http.Services;

namespace FclEx.Web.Core
{
    public interface IUserClientFactory<out TClient, in TAccount>
        where TClient : IUserClient, IHasAccount<TAccount>
    {
        IServiceProvider ServiceProvider { get; }
        TClient Create([DisallowNull] TAccount account, IHttpService? httpService = null);
    }
}