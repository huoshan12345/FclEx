// ReSharper disable InheritdocInvalidUsage
namespace FclEx.Http;

/// <summary>
/// Represents a pipeline action that builds, sends, and handles an HTTP request.
/// </summary>
/// <typeparam name="T">The final result type produced from the HTTP response.</typeparam>
public interface IHttpAction<T> : IPipelineAction<T>, IHttpResponseHandler<T>
{
    /// <summary>
    /// Gets the HTTP service used to send requests.
    /// </summary>
    IHttpService HttpService { get; }

    /// <summary>
    /// Gets the request URI.
    /// </summary>
    Uri Uri { get; }

    /// <summary>
    /// Gets the HTTP method.
    /// </summary>
    HttpMethod Method { get; }

    /// <summary>
    /// Gets whether non-success status codes should become operation errors.
    /// </summary>
    bool EnsureSuccessStatusCode
#if NET6_0_OR_GREATER
        => true;
#else
    { get; }
#endif

#if NET6_0_OR_GREATER
    Task<OperationResult<T>> IPipelineAction<T>.ExecuteCoreAsync(CancellationToken token)
        => DefaultHttpAction.ExecuteCoreAsync(this, token);
#endif

    /// <summary>
    /// Builds the request before it is sent.
    /// </summary>
    /// <returns>The request to send.</returns>
    HttpRequest BuildRequest()
#if NET6_0_OR_GREATER
        => DefaultHttpAction.BuildRequest(this);
#else
    ;
#endif

    /// <summary>
    /// Mutates the built request before it is sent.
    /// </summary>
    /// <param name="request">The request to modify.</param>
    void ModifyRequest(HttpRequest request)
#if NET6_0_OR_GREATER
    { }
#else
    ;
#endif

    /// <summary>
    /// Sends or otherwise obtains the HTTP response for a request.
    /// </summary>
    /// <param name="request">The built request.</param>
    /// <param name="token">The cancellation token for the send operation.</param>
    /// <returns>The HTTP response.</returns>
    Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
#if NET6_0_OR_GREATER
        => DefaultHttpAction.GetResponseAsync(this, request, token);
#else
    ;
#endif

    /// <summary>
    /// Handles the raw response before the final result is produced.
    /// </summary>
    /// <param name="response">The successful transport response.</param>
    /// <returns>A response result. Non-success status codes may become errors depending on <see cref="EnsureSuccessStatusCode"/>.</returns>
    Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultHttpAction.HandleResponseAsync(this, response);
#else
    ;
#endif
}

/// <summary>
/// Provides default behavior for <see cref="IHttpAction{T}"/>.
/// </summary>
public static class DefaultHttpAction
{
    /// <summary>
    /// Executes an HTTP action from request creation through response handling.
    /// </summary>
    /// <typeparam name="T">The final result type.</typeparam>
    /// <param name="action">The action to execute.</param>
    /// <param name="token">The cancellation token for the response operation.</param>
    /// <returns>The action result, or an error if sending, handling, or parsing fails.</returns>
    public static async Task<OperationResult<T>> ExecuteCoreAsync<T>(IHttpAction<T> action, CancellationToken token)
    {
        var logger = action.HttpService.Logger;
        HttpRequest? request = null;
        try
        {
            request = action.BuildRequest();
            var response = await action.GetResponseAsync(request, token);
            if (response.IsSuccess)
                return await action.HandleResponseAsync(response)
                    .Then(action.GetResultAsync);

            if (logger.IsEnabled(LogLevel.Trace))
            {
                var dump = request.Dump(action.HttpService);
                logger.LogTrace("{Dump}", dump);
            }

            return (response.Exception, response.Elapsed);
        }
        catch (Exception ex)
        {
            if (logger.IsEnabled(LogLevel.Trace) && request is not null)
            {
                var dump = request.Dump(action.HttpService);
                logger.LogTrace(ex, "{Dump}", dump);
            }
            return ex;
        }
    }

    /// <summary>
    /// Builds a default request from an HTTP action.
    /// </summary>
    /// <typeparam name="T">The final result type.</typeparam>
    /// <param name="action">The HTTP action.</param>
    /// <returns>A request with transport-level success enforcement disabled and compression accepted.</returns>
    public static HttpRequest BuildRequest<T>(IHttpAction<T> action)
    {
        var request = HttpRequest.Create(action.Uri, action.Method)
            .EnsureSuccessStatusCode(false)
            .AcceptCompress();
        action.ModifyRequest(request);
        return request;
    }

    /// <summary>
    /// Sends a request through the action's HTTP service.
    /// </summary>
    /// <typeparam name="T">The final result type.</typeparam>
    /// <param name="action">The HTTP action.</param>
    /// <param name="request">The request to send.</param>
    /// <param name="token">The cancellation token for the send operation.</param>
    /// <returns>The response returned by <see cref="IHttpService.SendAsync(HttpRequest, CancellationToken)"/>.</returns>
    public static Task<HttpResponse> GetResponseAsync<T>(IHttpAction<T> action, HttpRequest request, CancellationToken token)
    {
        return action.HttpService.SendAsync(request, token);
    }

    /// <summary>
    /// Converts an HTTP response into an operation result.
    /// </summary>
    /// <typeparam name="T">The final result type.</typeparam>
    /// <param name="action">The HTTP action.</param>
    /// <param name="response">The response to handle.</param>
    /// <returns>
    /// A successful response result when status enforcement is disabled or the status code is successful;
    /// otherwise an error result with the response elapsed time.
    /// </returns>
    public static Task<OperationResult<HttpResponse>> HandleResponseAsync<T>(IHttpAction<T> action, HttpResponse response)
    {
        if (action.EnsureSuccessStatusCode == false || response.StatusCode.IsSuccess())
            return Operation.Success(response, response.Elapsed);

        var code = response.StatusCode;
        var error = $"The response with status code {code}/{code.ToInt()} is unsuccessful: "
                    + response.ResponseString.Truncate(256);
        return Operation.Error<HttpResponse>(error, response.Elapsed);
    }
}

/// <summary>
/// Base class for HTTP pipeline actions.
/// </summary>
/// <typeparam name="T">The final result type produced from the HTTP response.</typeparam>
public abstract class HttpAction<T> : PipelineAction<T>, IHttpAction<T>
{
    /// <inheritdoc />
    public abstract IHttpService HttpService { get; }

    /// <inheritdoc />
    public abstract Uri Uri { get; }

    /// <inheritdoc />
    public abstract HttpMethod Method { get; }

    /// <inheritdoc />
    public virtual bool EnsureSuccessStatusCode => true;

    /// <inheritdoc />
    public virtual HttpRequest BuildRequest() => DefaultHttpAction.BuildRequest(this);

    /// <inheritdoc />
    public virtual void ModifyRequest(HttpRequest request) { }

    /// <inheritdoc />
    public virtual Task<HttpResponse> GetResponseAsync(HttpRequest request, CancellationToken token = default)
        => DefaultHttpAction.GetResponseAsync(this, request, token);

    /// <inheritdoc />
    public virtual Task<OperationResult<HttpResponse>> HandleResponseAsync(HttpResponse response)
        => DefaultHttpAction.HandleResponseAsync(this, response);

    /// <inheritdoc />
    public abstract OperationResult<T> GetResult(HttpResponse response);

    /// <inheritdoc />
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response)
        => DefaultHttpResponseHandler.GetResultAsync(this, response);

    /// <inheritdoc />
    public override Task<OperationResult<T>> ExecuteCoreAsync(CancellationToken token = default)
        => DefaultHttpAction.ExecuteCoreAsync(this, token);
}
