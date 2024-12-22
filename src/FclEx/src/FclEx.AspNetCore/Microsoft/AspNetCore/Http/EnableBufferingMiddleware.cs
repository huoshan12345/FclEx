namespace Microsoft.AspNetCore.Http;

/// <summary>
/// What we need to do here is call EnableBuffering() before the request reaches the MVC pipeline, so that the body stream is still available after the model binder has read from it.
/// </summary>
public class EnableBufferingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        await next(context);
    }
}