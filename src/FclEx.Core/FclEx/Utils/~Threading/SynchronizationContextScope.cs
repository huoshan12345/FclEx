namespace FclEx.Utils;

public static class SynchronizationContextScope
{
    // See: https://stackoverflow.com/questions/28305968/use-task-run-in-synchronous-method-to-avoid-deadlock-waiting-on-async-method
    public static IDisposable Enter(SynchronizationContext? ctx = null)
    {
        var current = SynchronizationContext.Current;
        ctx.Set();
        return Disposable.Create(current.Set);
    }

    public static T Run<T>(Func<Task<T>> action, SynchronizationContext? ctx = null)
    {
        using (Enter(ctx))
        {
            return action().GetAwaiter().GetResult();
        }
    }

    public static void Run(Func<Task> action, SynchronizationContext? ctx = null)
    {
        using (Enter(ctx))
        {
            action().GetAwaiter().GetResult();
        }
    }
}