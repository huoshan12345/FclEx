namespace FclEx.Extensions;

public static partial class ExceptionExtensions
{
    /// <summary>
    /// Re-throws the exception while preserving the original stack trace.
    /// </summary>
    /// <param name="ex">The exception to re-throw.</param>
    [StackTraceHidden, DoesNotReturn]
    public static void ReThrow(this Exception ex) => ExceptionDispatchInfo.Capture(ex).Throw();

    /// <summary>
    /// Gets the innermost exception from the exception chain.
    /// </summary>
    /// <param name="ex">The exception to examine.</param>
    /// <returns>The innermost exception from the chain.</returns>
    public static Exception GetInnermost(this Exception ex)
    {
        return ex.EnumerateInner().Last();
    }

    /// <summary>
    /// Enumerates through the chain of inner exceptions.
    /// </summary>
    /// <param name="ex">The exception to enumerate through.</param>
    /// <returns>An enumerable containing the exception and all its inner exceptions.</returns>
    public static IEnumerable<Exception> EnumerateInner(this Exception ex)
    {
        var p = ex;
        while (p != null)
        {
            yield return p;
            p = p.InnerException;
        }
    }

    /// <summary>
    /// Executes an action on each exception in the exception tree, including all inner exceptions
    /// and inner exceptions of AggregateExceptions.
    /// </summary>
    /// <param name="ex">The root exception to start from.</param>
    /// <param name="action">The action to execute on each exception.</param>
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

    /// <summary>
    /// Checks if the exception is an ObjectException of type T and extracts its value if it is.
    /// </summary>
    /// <typeparam name="T">The type of the value wrapped by the ObjectException.</typeparam>
    /// <param name="ex">The exception to check.</param>
    /// <param name="value">When this method returns, contains the value extracted from the ObjectException if the exception is an ObjectException{T}; 
    /// otherwise, the default value for type T.</param>
    /// <returns>True if the exception is an ObjectException{T}; otherwise, false.</returns>
    /// <remarks>
    /// This method provides a convenient way to check if an exception carries a specific type of data
    /// and to extract that data in a single operation.
    /// </remarks>
    public static bool IsObjectException<T>([NotNullWhen(true)] this Exception? ex, [NotNullWhen(true)] out T? value) where T : notnull
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

    /// <summary>
    /// Checks if the exception is an ObjectException of type T, extracts its value, and verifies if the value satisfies a specified condition.
    /// </summary>
    /// <typeparam name="T">The type of the value wrapped by the ObjectException.</typeparam>
    /// <param name="ex">The exception to check.</param>
    /// <param name="condition">A function that defines a condition to check against the extracted value.</param>
    /// <param name="value">When this method returns, contains the value extracted from the ObjectException if the exception is an ObjectException{T} 
    /// and the condition is satisfied; otherwise, the default value for type T.</param>
    /// <returns>True if the exception is an ObjectException{T} and the value satisfies the condition; otherwise, false.</returns>
    /// <remarks>
    /// This method combines type checking, value extraction, and condition validation in a single operation.
    /// If the exception is not an ObjectException{T} or the condition is not met, the method returns false.
    /// </remarks>
    public static bool IsObjectException<T>([NotNullWhen(true)] this Exception? ex, Func<T, bool> condition, [NotNullWhen(true)] out T? value) where T : notnull
    {
        return ex.IsObjectException(out value) && condition(value);
    }

    /// <summary>
    /// Checks if the exception is an ObjectException of type T that satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the ObjectException.</typeparam>
    /// <param name="ex">The exception to check.</param>
    /// <param name="condition">A function that defines a condition to check against the value.</param>
    /// <returns>True if the exception is an ObjectException of type T and the condition is met; otherwise, false.</returns>
    public static bool IsObjectException<T>([NotNullWhen(true)] this Exception? ex, Func<T, bool> condition) where T : notnull
    {
        return ex.IsObjectException(condition, out _);
    }

    /// <summary>
    /// Determines whether the exception is an OperationCanceledException, indicating a canceled operation.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception is an OperationCanceledException; otherwise, false.</returns>
    public static bool IsCanceled(this Exception ex)
    {
        return ex is OperationCanceledException;
    }

    /// <summary>
    /// Checks if the exception is essentially just a message; that is, it has no inner exception and no stack trace.
    /// This typically indicates that the exception object has been created but not actually thrown.
    /// In such cases, the primary useful information is the exception's message.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception is just a message (no inner exception or stack trace), false otherwise.</returns>
    public static bool IsJustMessage(this Exception ex)
    {
        return ex.InnerException == null && ex.StackTrace.IsNullOrEmpty();
    }

    /// <summary>
    /// Sets the message of an exception through reflection.
    /// </summary>
    /// <param name="ex">The exception to modify.</param>
    /// <param name="message">The new message to set.</param>
    /// <returns>The exception with its message modified.</returns>
    public static Exception SetMessage(this Exception ex, string? message)
    {
        FieldInfos.Exception_Message.SetValue(ex, message);
        return ex;
    }

    /// <summary>
    /// Sets the message of an exception using a function that takes the exception as input.
    /// </summary>
    /// <param name="ex">The exception to modify.</param>
    /// <param name="func">A function that takes the exception and returns a new message string.</param>
    /// <returns>The exception with its message modified.</returns>
    public static Exception SetMessage(this Exception ex, Func<Exception, string> func)
    {
        return ex.SetMessage(func(ex));
    }

    /// <summary>
    /// Gets the message of an exception through reflection.
    /// </summary>
    /// <param name="ex">The exception to examine.</param>
    /// <returns>The message of the exception.</returns>
    public static string? GetMessage(this Exception ex)
    {
        return FieldInfos.Exception_Message.GetValue<string>(ex);
    }

    /// <summary>
    /// Sets the stack trace of an exception through reflection.
    /// </summary>
    /// <param name="ex">The exception to modify.</param>
    /// <param name="trace">The new stack trace to set. If null, a new stack trace starting from the caller will be generated.</param>
    /// <returns>The exception with its stack trace modified.</returns>
    public static Exception SetStackTrace(this Exception ex, string? trace = null)
    {
        trace ??= new StackTrace(1, true).ToString();
        FieldInfos.Exception_StackTrace.SetValue(ex, trace);
        return ex;
    }

    /// <summary>
    /// Gets the stack trace of an exception through reflection.
    /// </summary>
    /// <param name="ex">The exception to examine.</param>
    /// <returns>The stack trace of the exception.</returns>
    public static string? GetStackTrace(this Exception ex)
    {
        return FieldInfos.Exception_StackTrace.GetValue<string>(ex);
    }
}