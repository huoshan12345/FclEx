using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http;

public abstract class AbstractHttpService : IHttpService
{
    protected readonly CookieContainer _cookieContainer;
    protected volatile IWebProxy? _webProxy;
    private ILogger _logger = NullLogger.Instance;

    protected AbstractHttpService(bool useCookie, IWebProxy? proxy = null, ILoggerFactory? loggerFactory = null)
    {
        WebProxy = proxy;
        loggerFactory ??= NullLoggerFactory.Instance;
        Logger = loggerFactory.CreateLogger(GetType());
        _cookieContainer = new CookieContainer();
        UseCookie = useCookie;
    }

    protected bool UseCookie { get; }

    public virtual void Dispose() { }

    protected abstract Task ExecuteAsyncInternal(HttpReq httpReq, HttpRes httpRes, CancellationToken token);

    public async Task<HttpRes> ExecuteAsync(HttpReq httpReq, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var watch = ValueStopwatch.StartNew();
        var res = new HttpRes(httpReq) { RequestUtcTime = DateTime.UtcNow };
        try
        {
            await ExecuteAsyncInternal(httpReq, res, token).DonotCapture();
        }
        catch (Exception e)
        {
            res.Exception = e;
        }
        finally
        {
            res.ExecuteTime = watch.GetElapsedTime();
        }
        return res;
    }

    public Cookie? GetCookie(Uri uri, string name)
    {
        return UseCookie
            ? _cookieContainer.GetCookies(uri)[name]
            : null;
    }

    public IReadOnlyCollection<Cookie> GetCookies(Uri uri)
    {
        return UseCookie
            ? _cookieContainer.GetCookies(uri)
            : Array.Empty<Cookie>();
    }

    public void AddCookie(Cookie cookie, Uri? uri = null)
    {
        if (!UseCookie) return;
        if (uri == null)
            _cookieContainer.Add(cookie);
        else
            _cookieContainer.Add(uri, cookie);
    }

    public IReadOnlyCollection<Cookie> GetAllCookies()
    {
        return UseCookie
            ? _cookieContainer.GetAllCookies()
            : Array.Empty<Cookie>();
    }

    public IWebProxy? WebProxy
    {
        get => _webProxy;
        set => SetProxy(value);
    }

    protected virtual void SetProxy(IWebProxy? proxy)
    {
        if (Equals(_webProxy, proxy)) 
            return;

        _webProxy = proxy;
    }

    public ILogger Logger
    {
        get => _logger = (_logger ?? NullLogger.Instance);
        set => _logger = value;
    }

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