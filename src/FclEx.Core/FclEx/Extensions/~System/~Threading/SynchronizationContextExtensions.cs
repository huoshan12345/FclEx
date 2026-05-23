namespace FclEx.Extensions;

public static class SynchronizationContextExtensions
{
    public static void Set(this SynchronizationContext? ctx)
    {
        SynchronizationContext.SetSynchronizationContext(ctx);
    }
}