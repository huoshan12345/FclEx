namespace FclEx.Utils;

/// <summary>
/// Creates timers without capturing the caller's <see cref="ExecutionContext"/>.
/// </summary>
public static class NonCapturingTimer
{
    public static Timer<T> Create<T>(TimerCallback<T> callback, T state, TimeSpan dueTime, TimeSpan period)
    {
        Check.NotNull(callback);

        return CreateTimer(() => new Timer<T>(callback, state, dueTime, period));
    }

    public static Timer Create(Action callback, TimeSpan dueTime, TimeSpan period)
    {
        Check.NotNull(callback);

        return CreateTimer(() => new Timer(callback, dueTime, period));
    }

    private static TTimer CreateTimer<TTimer>(Func<TTimer> create)
    {
        var restoreFlow = false;
        try
        {
            if (ExecutionContext.IsFlowSuppressed() == false)
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }
            return create();
        }
        finally
        {
            if (restoreFlow)
                ExecutionContext.RestoreFlow();
        }
    }
}
