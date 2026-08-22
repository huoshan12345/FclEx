namespace FclEx.Http;

/// <summary>
/// Base implementation of <see cref="IHttpService"/> with cookie storage and response timing/error capture.
/// </summary>
public abstract class HttpServiceBase : IHttpService
{
    protected readonly CookieContainer _cookieContainer = new();

    // ReSharper disable once MemberCanBeProtected.Global
    /// <summary>
    /// Whether the built-in cookie container should be used.
    /// </summary>
    /// <remarks>When disabled, adding cookies is ignored and cookie lookup methods return empty results.</remarks>
    public bool UseCookie { get; set; } = true;

    /// <inheritdoc />
    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected abstract Task ExecuteAsyncInternal(HttpRequest request, HttpResponse response, CancellationToken token);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Cookie? GetCookie(Uri uri, string name)
    {
        return UseCookie
            ? _cookieContainer.GetCookies(uri)[name]
            : null;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Cookie> GetCookies(Uri uri)
    {
        return UseCookie
#if !NET5_0_OR_GREATER
            ? _cookieContainer.GetCookies(uri).Enumerate().AsIReadOnlyCollection()
#else
            ? _cookieContainer.GetCookies(uri)
#endif
            : [];
    }

    /// <inheritdoc />
    public void AddCookie(Cookie cookie, Uri? uri = null, bool overrideDomain = false)
    {
        if (UseCookie == false)
            return;

        if (uri == null)
        {
            _cookieContainer.Add(cookie);
            return;
        }

        if (overrideDomain)
        {
            cookie = cookie.Clone();
            cookie.Domain = uri.Host;
        }

        _cookieContainer.Add(uri, cookie);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Cookie> GetAllCookies()
    {
        return UseCookie
#if !NET5_0_OR_GREATER
            ? _cookieContainer.GetAllCookies().Enumerate().AsIReadOnlyCollection()
#else
            ? _cookieContainer.GetAllCookies()
#endif
            : [];
    }

    /// <inheritdoc />
    public virtual IWebProxy? Proxy { get; set; }

    /// <inheritdoc />
    [AllowNull]
    public ILogger Logger
    {
        get;
        set => field = value ?? NullLogger.Instance;
    } = NullLogger.Instance;

    protected void SaveCookies(Uri responseUri, string cookieStr)
    {
        if (UseCookie == false)
            return;

        try
        {
            var cookies = Cookie.Parse(cookieStr);
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
                    Logger.LogWarning(ex, "{Error}", ex?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning("Failed to parse cookie due to {Error}", ex.Message);
        }
    }

    protected void SaveCookies(Uri? responseUri, IEnumerable<string> cookieStrings)
    {
        if (UseCookie == false || responseUri is null)
            return;

        foreach (var cookieStr in cookieStrings)
        {
            try
            {
                SaveCookies(responseUri, cookieStr);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("A cookie has been discarded due to {Error}: {Cookie}", ex.Message, cookieStr);
            }
        }
    }
}
