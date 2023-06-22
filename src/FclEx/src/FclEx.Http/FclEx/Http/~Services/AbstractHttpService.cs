using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace FclEx.Http;

public abstract class AbstractHttpService : IHttpService
{
    protected readonly CookieContainer _cookieContainer = new();
    private ILogger _logger = NullLogger.Instance;

    public bool UseCookie { get; set; } = true;

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected abstract Task ExecuteAsyncInternal(HttpRequest request, HttpResponse response, CancellationToken token);

    public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var watch = ValueStopwatch.StartNew();
        var res = new HttpResponse(request) { StartTime = DateTimeOffset.UtcNow };
        try
        {
            await ExecuteAsyncInternal(request, res, token).DonotCapture();
        }
        catch (Exception e)
        {
            res.Exception = e;
        }
        finally
        {
            res.Elapsed = watch.GetElapsedTime();
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

    public virtual IWebProxy? Proxy { get; set; }

    [AllowNull]
    public ILogger Logger
    {
        get => _logger;
        set => _logger = value ?? NullLogger.Instance;
    }

    protected void SaveCookies(Uri responseUri, string cookieStr)
    {
        if (UseCookie == false)
            return;

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
        if (UseCookie == false)
            return;

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