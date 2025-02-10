namespace FclEx.Http;

public static class Extensions
{
    public static IAction<HttpResponse> ToAction(this HttpRequest request, IHttpService? httpService = null, bool unwrapError = true)
    {
        return new HttpRequestAction(request, httpService ?? HttpClientService.Default, unwrapError);
    }

    public static IAction<T> ReadJson<T>(this IAction<HttpResponse> action, string? path = null)
    {
        return action.Bind(m => m.ReadJsonAs<T>(path));
    }

    public static IAction<HttpResponse> NextRequest<T>(this IAction<(HttpResponse, T)> action, Func<T, HttpRequest> func,
        IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(func);
        return action.Next((_, data) => func(data).ToAction(httpService, unwrapError));
    }

    public static IAction<HttpResponse> NextRequest<T>(this IAction<T> action, Func<T, HttpRequest> func,
        IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(func);
        return action.Next(data => func(data).ToAction(httpService, unwrapError));
    }

    public static IAction<HttpResponse>? TryRedirect(this HttpResponse response, IHttpService httpService, Func<HttpResponse, string?> urlFunc)
    {
        Check.NotNull(urlFunc);
        var url = urlFunc(response);
        return url == null ? null : HttpRequest.Get(url).ToAction(httpService);
    }

    public static IAction<HttpResponse>? TryRedirect(this HttpResponse response, IHttpService httpService, string? url)
    {
        return response.TryRedirect(httpService, r => url);
    }

    public static IAction<HttpResponse> NextRequest<T>(this IAction<T> action, HttpRequest request, IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(request);
        return action.NextRequest(m => request, httpService, unwrapError);
    }
}