namespace FclEx.Extensions;

public static partial class ExceptionExtensions
{

#if NET8_0_OR_GREATER
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_sendStatus")]
    private static extern ref int HttpRequestMessageSendStatus(HttpRequestMessage request);
#endif

    /// <summary>
    /// Re-throws the exception while preserving the original stack trace.
    /// </summary>
    /// <param name="ex">The exception to re-throw.</param>
    [StackTraceHidden, DoesNotReturn]
    [MethodImpl(AggressiveInlining)]
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
    /// Enumerates the leaf exceptions in an exception tree.
    /// </summary>
    /// <param name="ex">The root exception.</param>
    /// <returns>Each exception without child exceptions, at most once, in breadth-first order.</returns>
    public static IEnumerable<Exception> EnumerateLeaves(this Exception ex)
    {
        var visited = new HashSet<Exception>(ReferenceEqualityComparer<Exception>.Instance) { ex };
        var q = new Queue<Exception>();
        q.Enqueue(ex);

        while (q.Count != 0)
        {
            var e = q.Dequeue();

            if (EnqueueChildren(e, q, visited) == false)
                yield return e;
        }
    }

    /// <summary>
    /// Enumerates an exception tree, including aggregate inner exceptions.
    /// </summary>
    /// <param name="ex">The root exception.</param>
    /// <returns>Each exception in breadth-first order, at most once.</returns>
    public static IEnumerable<Exception> Enumerate(this Exception ex)
    {
        var visited = new HashSet<Exception>(ReferenceEqualityComparer<Exception>.Instance) { ex };
        var q = new Queue<Exception>();
        q.Enqueue(ex);

        while (q.Count != 0)
        {
            var e = q.Dequeue();

            yield return e;
            EnqueueChildren(e, q, visited);
        }
    }

    private static bool EnqueueChildren(Exception exception, Queue<Exception> queue, HashSet<Exception> visited)
    {
        if (exception is AggregateException { InnerExceptions.Count: > 0 } aggregateException)
        {
            foreach (var aggregateInnerException in aggregateException.InnerExceptions)
            {
                if (visited.Add(aggregateInnerException))
                    queue.Enqueue(aggregateInnerException);
            }

            return true;
        }

        if (exception.InnerException is { } innerException)
        {
            if (visited.Add(innerException))
                queue.Enqueue(innerException);

            return true;
        }

        return false;
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
        if (ex is IValueProvider<T> valueProvider)
        {
            value = valueProvider.Value;
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
    /// Replaces the raw message stored by the base Exception implementation.
    /// </summary>
    /// <param name="ex">The exception to modify.</param>
    /// <param name="message">The raw message to store, including null to restore the base getter's default message.</param>
    /// <returns>The exception with its message modified.</returns>
    /// <remarks>
    /// This method accesses a non-public runtime field using an unsafe accessor on .NET 8 and later and reflection
    /// on older targets. It bypasses virtual property overrides and depends on the runtime's field layout.
    /// Reflection paths require the field to be preserved when trimming. Custom exception getters may ignore this field.
    /// </remarks>
    public static Exception SetMessage(this Exception ex, string? message)
    {
#if NET8_0_OR_GREATER
        ref var m = ref UnsafeAccessors.Exception_Message(ex);
        m = message;
#else
        FieldInfos.Exception_Message.SetValue(ex, message);
#endif
        return ex;
    }

    /// <summary>
    /// Sets the message of an exception using a function that takes the exception as input.
    /// </summary>
    /// <param name="ex">The exception to modify.</param>
    /// <param name="func">A function that takes the exception and returns a new message string.</param>
    /// <returns>The exception with its message modified.</returns>
    /// <remarks>Uses <see cref="SetMessage(Exception, string?)"/> and has the same private-field dependency.</remarks>
    public static Exception SetMessage(this Exception ex, Func<Exception, string> func)
    {
        return ex.SetMessage(func(ex));
    }

    /// <summary>
    /// Reads the nullable raw message stored by the base Exception implementation.
    /// </summary>
    /// <param name="ex">The exception to examine.</param>
    /// <returns>The raw message, or null when unset; this may differ from the virtual Message getter.</returns>
    /// <remarks>Reads a non-public runtime field on <see cref="Exception"/>; see <see cref="SetMessage(Exception, string?)"/> for compatibility limitations.</remarks>
    public static string? GetMessage(this Exception ex)
    {
#if NET8_0_OR_GREATER
        return UnsafeAccessors.Exception_Message(ex);
#else
        return FieldInfos.Exception_Message.GetValue<string?>(ex);
#endif
    }

    /// <summary>
    /// Replaces the cached stack-trace string stored by the base Exception implementation.
    /// </summary>
    /// <param name="ex">The exception to modify.</param>
    /// <param name="stackTrace">The new stack trace to set. If null, a new stack trace starting from the caller will be generated.</param>
    /// <returns>The exception with its stack trace modified.</returns>
    /// <remarks>
    /// Uses an unsafe accessor on .NET 8 and later and reflection on older targets, depending on private runtime layout.
    /// This changes the cached string without clearing other captured stack data or changing virtual property overrides.
    /// A later throw can replace the cached string. Reflection paths require preserving the field when trimming.
    /// </remarks>
    public static Exception SetStackTrace(this Exception ex, string? stackTrace = null)
    {
        stackTrace ??= new StackTrace(1, true).ToString();
#if NET8_0_OR_GREATER
        ref var m = ref UnsafeAccessors.Exception_StackTrace(ex);
        m = stackTrace;
#else
        FieldInfos.Exception_StackTrace.SetValue(ex, stackTrace);
#endif
        return ex;
    }

    /// <summary>
    /// Reads the cached stack-trace string stored by the base Exception implementation.
    /// </summary>
    /// <param name="ex">The exception to examine.</param>
    /// <returns>The cached string, or null when unset, even if the public StackTrace getter can render captured stack data.</returns>
    /// <remarks>Reads a non-public runtime field on <see cref="Exception"/>; see <see cref="SetStackTrace(Exception, string?)"/> for compatibility limitations.</remarks>
    public static string? GetStackTrace(this Exception ex)
    {
#if NET8_0_OR_GREATER
        return UnsafeAccessors.Exception_StackTrace(ex);
#else
        return FieldInfos.Exception_StackTrace.GetValue<string?>(ex);
#endif
    }
}
