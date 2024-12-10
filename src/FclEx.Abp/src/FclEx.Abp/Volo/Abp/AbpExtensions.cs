namespace Volo.Abp;

public static class AbpExtensions
{
    public static T? GetObject<T>(this ApplicationInitializationContext context)
    {
        return context.ServiceProvider.GetObject<T>();
    }
}