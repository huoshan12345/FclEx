namespace FclEx.Extensions;

public static partial class ExceptionExtensions
{
    [StackTraceHidden, DoesNotReturn]
    public static void ReThrow(this Exception ex) => ExceptionDispatchInfo.Capture(ex).Throw();

    public static Exception GetInnermost(this Exception ex)
    {
        return ex.EnumerateInner().Last();
    }

    public static IEnumerable<Exception> EnumerateInner(this Exception ex)
    {
        var p = ex;
        while (p != null)
        {
            yield return p;
            p = p.InnerException;
        }
    }

    public static void ForEach(this Exception? ex, Action<Exception>? action)
    {
        if (ex is null || action is null)
            return;

        var q = new Queue<Exception>();
        q.Enqueue(ex);
        var handled = new HashSet<Exception>();
        while (q.Count != 0)
        {
            var e = q.Dequeue();
            if (e is AggregateException aEx)
            {
                foreach (var inner in aEx.InnerExceptions)
                {
                    EnqueueIfUnHandled(inner);
                }
            }
            else if (e.InnerException is not null)
            {
                EnqueueIfUnHandled(e.InnerException);
            }
            else
            {
                try
                {
                    action(e);
                }
                finally
                {
                    handled.Add(e);
                }
            }
        }
        handled.Clear();
        return;

        void EnqueueIfUnHandled(Exception exception)
        {
            if (handled.Contains(exception))
                return;
            q.Enqueue(exception);
        }
    }

    public static Exception Unwrap(this Exception ex)
    {
        if (ex is AggregateException agg)
        {
            agg = agg.Flatten(); // agg does not contain AggregateException now.
            if (agg.InnerExceptions.Count == 1)
                return agg.InnerExceptions[0];
        }
        return ex;
    }

    public static bool IsObjEx<T>([NotNullWhen(true)] this Exception? ex, [NotNullWhen(true)] out T? value) where T : notnull
    {
        if (ex is ObjectException<T> objEx)
        {
            value = objEx.Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    public static bool IsObjEx<T>([NotNullWhen(true)] this Exception? ex, Func<T, bool> condition) where T : notnull
    {
        return ex is ObjectException<T> objEx && condition(objEx.Value);
    }

    public static Exception SetMessage(this Exception ex, string? message)
    {
        Fields.Exception_Message.SetValue(ex, message);
        return ex;
    }

    public static Exception SetMessage(this Exception ex, Func<Exception, string> func)
    {
        return ex.SetMessage(func(ex));
    }

    public static string? GetMessage(this Exception ex)
    {
        return Fields.Exception_Message.GetValue<string>(ex);
    }

    public static Exception SetStackTrace(this Exception ex, string? trace = null)
    {
        trace ??= new StackTrace(1, true).ToString();
        Fields.Exception_StackTrace.SetValue(ex, trace);
        return ex;
    }

    public static string? GetStackTrace(this Exception ex)
    {
        return Fields.Exception_StackTrace.GetValue<string>(ex);
    }
}