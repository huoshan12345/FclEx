namespace FclEx.DependencyInjection;

public static class ServiceScopeExtensions
{
    public static T GetRequiredService<T>(this IServiceScope scope) where T : notnull
    {
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
