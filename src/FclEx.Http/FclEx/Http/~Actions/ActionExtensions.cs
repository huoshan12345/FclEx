namespace FclEx.Http;

/// <summary>
/// Provides action helpers for composing HTTP requests and response parsing.
/// </summary>
public static class ActionExtensions
{
    /// <summary>
    /// Reads a JSON response action as a typed value.
    /// </summary>
    /// <typeparam name="T">The value type to deserialize.</typeparam>
    /// <param name="action">The response action.</param>
    /// <param name="path">The optional JSON path to read. When <see langword="null"/>, the root value is used.</param>
    /// <returns>An action that returns the deserialized value or the response/parsing error.</returns>
    public static IAction<T> ReadJsonAs<T>(this IAction<HttpResponse> action, string? path = null)
    {
        return action.MapResult(m => m.ReadJsonAs<T>(path));
    }

    /// <summary>
    /// Sends another HTTP request after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="func">Creates the next request from the source value. It is not called when the source action fails.</param>
    /// <param name="httpService">The service used to send the next request. Uses the default service when <see langword="null"/>.</param>
    /// <param name="unwrapError">Whether a failed response from the next request should become an error result.</param>
    /// <returns>An action that sends the created request.</returns>
    public static IAction<HttpResponse> ThenRequest<T>(this IAction<T> action, Func<T, HttpRequest> func, IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(func);
        return action.Then(m => func(m).ToAction(httpService, unwrapError));
    }

    /// <summary>
    /// Sends the given HTTP request after this action succeeds.
    /// </summary>
    /// <typeparam name="T">The source action value type.</typeparam>
    /// <param name="action">The source action.</param>
    /// <param name="request">The request to send after success.</param>
    /// <param name="httpService">The service used to send the request. Uses the default service when <see langword="null"/>.</param>
    /// <param name="unwrapError">Whether a failed response from the request should become an error result.</param>
    /// <returns>An action that sends <paramref name="request"/> after the source action succeeds.</returns>
    public static IAction<HttpResponse> ThenRequest<T>(this IAction<T> action, HttpRequest request, IHttpService? httpService = null, bool unwrapError = true)
    {
        Check.NotNull(request);
        return action.ThenRequest(m => request, httpService, unwrapError);
    }
}
