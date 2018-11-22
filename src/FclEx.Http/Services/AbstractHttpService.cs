using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FclEx.Http.Core;
using FclEx.Http.Core.Cookies;
using FclEx.Http.Proxy;
using FclEx.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http.Services
{
    public abstract class AbstractHttpService : IHttpService
    {
        static AbstractHttpService()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        protected readonly CookieContainer _cookieContainer;
        protected IWebProxyExt _webProxy = WebProxyExt.None;

        protected AbstractHttpService(
            bool useCookie,
            IWebProxyExt proxy = null,
            ILoggerFactory loggerFactory = null)
        {
            WebProxy = proxy;
            loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
            Logger = loggerFactory.CreateLogger(GetType());
            if (useCookie)
                _cookieContainer = new CookieContainer();
        }

        protected bool UseCookie => _cookieContainer != null;

        public virtual void Dispose()
        {
        }

        public abstract ValueTask<HttpRes> ExecuteAsync(
            HttpReq httpReq,
            CancellationToken token = default);

        public Cookie GetCookie(Uri uri, string name)
        {
            return UseCookie
                ? _cookieContainer.GetCookies(uri)[name]
                : null;
        }

        public IReadOnlyList<Cookie> GetCookies(Uri uri)
        {
            return UseCookie
                ? _cookieContainer.GetCookies(uri).OfType<Cookie>().ToArray()
                : Array.Empty<Cookie>();
        }

        public void AddCookie(Cookie cookie, Uri uri)
        {
            if (UseCookie)
                _cookieContainer.Add(uri, cookie);
        }

        public IReadOnlyList<Cookie> GetAllCookies()
        {
            return UseCookie
                ? (IReadOnlyList<Cookie>)_cookieContainer.GetAllCookies()
                : Array.Empty<Cookie>();
        }

        public IWebProxyExt WebProxy
        {
            get => _webProxy;
            set => SetProxy(value);
        }

        protected virtual void SetProxy(IWebProxyExt proxy)
        {
            if (Equals(_webProxy, proxy)) return;
            _webProxy = proxy ?? WebProxyExt.None;
        }

        public ILogger Logger { get; }

        protected void SaveCookies(Uri responseUri, string cookieStr)
        {
            if (!UseCookie) return;
            try
            {
                var parser = new CookieParser(cookieStr);
                while (true)
                {
                    var c = parser.Get();
                    if (c == null) break;
                    if (c.Name.IsNullOrEmpty())
                    {
                        Logger.LogWarning("A cookie has been rejected: " + c);
                        continue;
                    }

                    try
                    {
                        var cookie = c.ToCookie();
                        if (cookie.Domain.IsNullOrEmpty())
                            _cookieContainer.Add(responseUri, cookie);
                        else
                            _cookieContainer.Add(cookie);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "A cookie has been discarded: " + c);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning("An error occurred while parsing cookie. " + ex.Message);
            }
        }

        protected void SaveCookies(Uri responseUri, IEnumerable<string> cookieStrs)
        {
            if (!UseCookie) return;
            foreach (var cookieStr in cookieStrs)
            {
                try
                {
                    SaveCookies(responseUri, cookieStr);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"A cookie has been discarded. [{cookieStr}][{ex.Message}]");
                }
            }
        }
    }
}
