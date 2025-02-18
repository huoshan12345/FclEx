namespace FclEx.Http;

public abstract class AbstractHttpService : IHttpService
{
    protected readonly CookieContainer _cookieContainer = new();
    private ILogger _logger = NullLogger.Instance;

    // ReSharper disable once MemberCanBeProtected.Global
    public bool UseCookie { get; set; } = true;

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected abstract Task ExecuteAsyncInternal(HttpRequest httpRequest, HttpResponse httpResponse, CancellationToken token);

    public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var watch = ValueStopwatch.StartNew();
        var response = new HttpResponse(request) { StartTime = DateTimeOffset.UtcNow };
        try
        {
            await ExecuteAsyncInternal(request, response, token);
        }
        catch (Exception e)
        {
            response.Exception = e;
        }
        finally
        {
            response.Elapsed = watch.GetElapsedTime();
        }
        return response;
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
#if NETSTANDARD2_0
            ? _cookieContainer.GetCookies(uri).Enumerate().AsIReadOnlyCollection()
#else
            ? _cookieContainer.GetCookies(uri)
#endif
            : Array.Empty<Cookie>();
    }

    public void AddCookie(Cookie cookie, Uri? uri = null)
    {
        if (UseCookie == false)
            return;

        if (uri == null)
            _cookieContainer.Add(cookie);
        else
            _cookieContainer.Add(uri, cookie);
    }

    public IReadOnlyCollection<Cookie> GetAllCookies()
    {
        return UseCookie
#if NETSTANDARD2_0
            ? _cookieContainer.GetAllCookies().Enumerate().AsIReadOnlyCollection()
#else
            ? _cookieContainer.GetAllCookies()
#endif
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
            var cookies = CookieHelper.Parse(cookieStr);
            foreach (var (success, cookie, ex, _) in cookies)
            {
                if (success)
                {
                    if (cookie!.Domain.IsNullOrEmpty())
                    {
                        _cookieContainer.Add(responseUri, cookie);
                    }
                    else
                    {
                        _cookieContainer.Add(cookie);
                    }
                }
                else
                {
                    Logger.LogWarning(ex, ex!.Message);
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