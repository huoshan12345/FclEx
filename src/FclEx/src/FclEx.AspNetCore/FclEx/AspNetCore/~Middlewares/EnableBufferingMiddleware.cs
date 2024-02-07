namespace FclEx.AspNetCore;

/// <summary>
/// What we need to do here is call EnableBuffering() before the request reaches the MVC pipeline, so that the body stream is still available after the model binder has read from it.
/// </summary>
public class EnableBufferingMiddleware
{
    private readonly RequestDelegate _next;

    public EnableBufferingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        await _next(context);
    }
}