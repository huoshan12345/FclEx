using Volo.Abp;

namespace FclEx.Abp;

public static class ApplicationInitializationContextExtensions
{
    public static T? GetObject<T>(this ApplicationInitializationContext context)
    {
        return context.ServiceProvider.GetObject<T>();
    }
}