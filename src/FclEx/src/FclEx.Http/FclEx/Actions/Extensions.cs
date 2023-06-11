namespace FclEx.Actions;

public static class Extensions
{
    public static IAction<HttpRes> ToAction(this HttpReq req, IHttpService? httpService = null, bool unwrapError = true)
    {
        return (new HttpReqAction(req, httpService ?? HttpClientService.Default, unwrapError));
    }

    public static IAction<T> ReadJson<T>(this IAction<HttpRes> action, string? path = null)
    {
        return action.Bind(m => m.ReadJson<T>(path));
    }

    public static IAction<HttpRes> NextReq<T>(this IAction<(HttpRes, T)> action, Func<T, HttpReq> func,
        IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(func);
        return action.Next((res, data) => func(data).ToAction(httpService, unwrapError));
    }

    public static IAction<HttpRes> NextReq<T>(this IAction<T> action, Func<T, HttpReq> func,
        IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(func);
        return action.Next(data => func(data).ToAction(httpService, unwrapError));
    }

    public static IAction<HttpRes>? TryRedirect(this HttpRes res, IHttpService httpService, Func<HttpRes, string?> urlFunc)
    {
        Check.NotNull(urlFunc);
        var url = urlFunc(res);
        return url == null ? null : HttpReq.Get(url).ToAction(httpService);
    }

    public static IAction<HttpRes>? TryRedirect(this HttpRes res, IHttpService httpService, string? url)
    {
        return res.TryRedirect(httpService, r => url);
    }

    public static IAction<HttpRes> NextReq<T>(this IAction<T> action, HttpReq httpReq, IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(httpReq);
        return action.NextReq(m => httpReq, httpService, unwrapError);
    }
}