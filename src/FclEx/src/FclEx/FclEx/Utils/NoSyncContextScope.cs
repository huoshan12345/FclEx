using System.Threading;
using System.Threading.Tasks;

namespace FclEx.Utils;

public static class NoSyncContextScope
{
    // See: https://stackoverflow.com/questions/28305968/use-task-run-in-synchronous-method-to-avoid-deadlock-waiting-on-async-method
    public static IDisposable Enter()
    {
        var context = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        return new Disposable(context);
    }

    private readonly struct Disposable : IDisposable
    {
        private readonly SynchronizationContext? _context;

        public Disposable(SynchronizationContext? context)
        {
            _context = context;
        }

        public void Dispose()
        {
            SynchronizationContext.SetSynchronizationContext(_context);
        }
    }

    public static T Run<T>(Func<Task<T>> action)
    {
        using (Enter())
        {
            return action().GetAwaiter().GetResult();
        }
    }

    public static void Run(Func<Task> action)
    {
        using (Enter())
        {
            action().GetAwaiter().GetResult();
        }
    }

    public static async Task RunAsync(Func<Task> action)
    {
        using (Enter())
        {
            await action();
        }
    }

    public static async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        using (Enter())
        {
            return await action();
        }
    }
}