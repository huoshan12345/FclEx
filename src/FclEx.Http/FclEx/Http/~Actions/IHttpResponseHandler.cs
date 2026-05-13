namespace FclEx.Http;

public interface IHttpResponseHandler<T>
{
    OperationResult<T> GetResult(HttpResponse response);
    Task<OperationResult<T>> GetResultAsync(HttpResponse response)
#if NET6_0_OR_GREATER
        => DefaultHttpResponseHandler.GetResultAsync(this, response);
#else
    ;
#endif
}

public static class DefaultHttpResponseHandler
{
    public static Task<OperationResult<T>> GetResultAsync<T>(IHttpResponseHandler<T> handler, HttpResponse response)
    {
        return handler.GetResult(response);
    }
}

public abstract class HttpResponseHandler<T> : IHttpResponseHandler<T>
{
    public abstract OperationResult<T> GetResult(HttpResponse response);
    public virtual Task<OperationResult<T>> GetResultAsync(HttpResponse response)
        => DefaultHttpResponseHandler.GetResultAsync(this, response);
}