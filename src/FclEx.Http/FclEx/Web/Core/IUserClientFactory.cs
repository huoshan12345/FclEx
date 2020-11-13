using System;
using System.Diagnostics.CodeAnalysis;
using FclEx.Http.Services;
using FclEx.Web.Models;

namespace FclEx.Web.Core
{
    public interface IUserClientFactory<out TClient> where TClient : IUserClient
    {
        IServiceProvider ServiceProvider { get; }
        TClient Create(IUserAccount account, IHttpService? httpService = null);
    }
}