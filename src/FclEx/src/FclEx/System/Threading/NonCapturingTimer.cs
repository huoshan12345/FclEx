using FclEx.Helpers;

namespace System.Threading;

// A convenience API for interacting with System.Threading.Timer in a way
// that doesn't capture the ExecutionContext. We should be using this (or equivalent)
// everywhere we use timers to avoid rooting any values stored in asynclocals.
public static class NonCapturingTimer
{
    public static Timer<T> Create<T>(TimerCallback<T> callback, T state, TimeSpan dueTime, TimeSpan period)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
        var restoreFlow = false;
        try
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }
            return new Timer<T>(callback, state, dueTime, period);
        }
        finally
        {
            // Restore the current ExecutionContext
            if (restoreFlow)
                ExecutionContext.RestoreFlow();
        }
    }

    public static StatelessTimer Create(StatelessTimerCallback callback, TimeSpan dueTime, TimeSpan period)
    {
        if (callback == null)
        {
            throw new ArgumentNullException(nameof(callback));
        }

        // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
        var restoreFlow = false;
        try
        {
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }
            return new StatelessTimer(callback, dueTime, period);
        }
        finally
        {
            // Restore the current ExecutionContext
            if (restoreFlow)
                ExecutionContext.RestoreFlow();
        }
    }
}