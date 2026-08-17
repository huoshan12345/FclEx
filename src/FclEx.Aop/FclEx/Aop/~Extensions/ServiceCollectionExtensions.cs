namespace FclEx.Aop;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAop(this IServiceCollection services)
    {
        services.Replace<IServiceProviderFactory<IServiceCollection>>(new DynamicProxyServiceProviderFactory());
        return services;
    }
}
