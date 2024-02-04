namespace Microsoft.AspNetCore.Http;

public static class HttpContextExtensions
{
    public static T? GetService<T>(this HttpContext context) where T : notnull
    {
        return context.RequestServices.GetService<T>();
    }

    public static T GetRequiredService<T>(this HttpContext context) where T : notnull
    {
        return context.RequestServices.GetRequiredService<T>();
    }
}