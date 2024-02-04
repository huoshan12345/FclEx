namespace Microsoft.AspNetCore.Mvc;

public static class ActionContextExtensions
{
    public static T? GetService<T>(this ActionContext context) where T : notnull
    {
        return context.HttpContext.GetService<T>();
    }

    public static T GetRequiredService<T>(this ActionContext context) where T : notnull
    {
        return context.HttpContext.GetRequiredService<T>();
    }
}