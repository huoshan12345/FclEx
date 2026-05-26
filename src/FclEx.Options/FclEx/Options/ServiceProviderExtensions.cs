namespace FclEx.Options;

public static class ServiceProviderExtensions
{
    public static T GetOptions<T>(this IServiceProvider provider) where T : class, new()
    {
        return provider.GetRequiredService<IOptions<T>>().Value;
    }
}
