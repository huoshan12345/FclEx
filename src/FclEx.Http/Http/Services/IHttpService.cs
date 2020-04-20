using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Proxy;
using Microsoft.Extensions.Logging;

namespace FclEx.Http.Services
{
    public interface IHttpService : IDisposable
    {
        Task<HttpRes> ExecuteAsync(HttpReq httpReq, CancellationToken token = default);

        void AddCookie(Cookie cookie, Uri? uri = null);

        Cookie? GetCookie(Uri uri, string name);

        IReadOnlyList<Cookie> GetCookies(Uri uri);

        IReadOnlyList<Cookie> GetAllCookies();

        IWebProxyExt WebProxy { get; set; }

        ILogger Logger { get; set; }
    }
}
