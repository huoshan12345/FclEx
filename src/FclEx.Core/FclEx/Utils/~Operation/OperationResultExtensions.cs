namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    /// <summary>
    /// Deconstructs a non-generic operation result into success, exception, and elapsed values.
    /// </summary>
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? exception, out TimeSpan elapsed)
    {
        success = result.IsSuccess;
        elapsed = result.Elapsed;
        exception = result.Exception;
    }

    /// <summary>
    /// Deconstructs a non-generic operation result into success and exception values.
    /// </summary>
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? exception)
    {
        success = result.IsSuccess;
        exception = result.Exception;
    }

    /// <summary>
    /// Merges multiple <see cref="OperationResult"/> instances into a single result.
    /// </summary>
    /// <param name="enumerable">The collection of <see cref="OperationResult"/> instances to merge.</param>
    /// <returns>
    /// A success result when all inputs are successful, an error result with the single exception when one input fails,
    /// or an error result with an <see cref="AggregateException"/> when multiple inputs fail.
    /// </returns>
    /// <remarks>Elapsed times are summed.</remarks>
    public static OperationResult Merge(this IEnumerable<OperationResult> enumerable)
    {
        Check.NotNull(enumerable);

        var (exceptions, time) = enumerable.Aggregate((Exceptions: new List<Exception>(), Time: TimeSpan.Zero), (seed, m) =>
        {
            var t = seed.Time + m.Elapsed;
            var e = m.Exception;
            return (e is null ? seed.Exceptions : seed.Exceptions.Push(e), t);
        });

        return exceptions.Count switch
        {
            0 => Operation.Success(time),
            1 => (exceptions[0], time),
            _ => (new AggregateException(exceptions), time)
        };
    }

    /// <summary>
    /// Determines whether the result is an object error with an associated object that satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The expected type of the associated object.</typeparam>
    /// <param name="result">The operation result to check.</param>
    /// <param name="condition">A function that determines whether the error condition is met.</param>
    /// <param name="value">The extracted object associated with the exception, if present.</param>
    /// <returns>
    /// <see langword="true"/> if the exception has an associated object of type <typeparamref name="T"/> 
    /// and the condition evaluates to <see langword="true"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsObjectError<T>(this IOperationResult result, Func<T, Exception, bool> condition, [NotNullWhen(true)] out T? value) where T : notnull
    {
        Check.NotNull(result);
        Check.NotNull(condition);

        return result.Exception.IsObjectException(out value) && condition(value, result.Exception);
    }

    /// <summary>
    /// Determines whether the result is an object error with an associated object of a specific type.
    /// </summary>
    /// <typeparam name="T">The expected type of the associated object.</typeparam>
    /// <param name="result">The operation result to check.</param>
    /// <param name="value">The extracted object associated with the exception, if present.</param>
    /// <returns>
    /// <see langword="true"/> if the exception has an associated object of type <typeparamref name="T"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsObjectError<T>(this IOperationResult result, [NotNullWhen(true)] out T? value) where T : notnull
    {
        Check.NotNull(result);
        return result.Exception.IsObjectException(out value);
    }

    /// <summary>
    /// Determines whether the result is an object error with an associated object that satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The expected type of the associated object.</typeparam>
    /// <param name="result">The operation result to check.</param>
    /// <param name="condition">A function that determines whether the error condition is met.</param>
    /// <returns>
    /// <see langword="true"/> if the exception has an associated object of type <typeparamref name="T"/> 
    /// and the condition evaluates to <see langword="true"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsObjectError<T>(this IOperationResult result, Func<T, Exception, bool> condition) where T : notnull
    {
        return result.IsObjectError(condition, out _);
    }

    /// <summary>
    /// Determines whether the result is an error represented by a cancellation exception.
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns>
    /// <see langword="true"/> if the result is an error and its exception represents a canceled operation;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsCanceled(this IOperationResult result)
    {
        Check.NotNull(result);

        return result.IsError && result.Exception.IsCanceled();
    }

    /// <summary>
    /// Determines whether the result is an error that is not cancellation.
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns><see langword="true"/> when the result is faulted; otherwise, <see langword="false"/>.</returns>
    public static bool IsFaulted(this IOperationResult result)
    {
        Check.NotNull(result);

        return result.IsError && !result.IsCanceled();
    }

    /// <summary>
    /// Determines whether the result is an error represented by a simple string message.
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns><see langword="true"/> when the result is a string error; otherwise, <see langword="false"/>.</returns>
    public static bool IsStringError(this IOperationResult result)
    {
        Check.NotNull(result);

        return result.IsError && result.Exception.IsJustMessage();
    }

    /// <summary>
    /// Determines whether the result is an error that is not represented by a simple string message.
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns><see langword="true"/> when the result is a non-string error; otherwise, <see langword="false"/>.</returns>
    public static bool IsNonStringError(this IOperationResult result)
    {
        Check.NotNull(result);

        return result.IsError && result.Exception.IsJustMessage() == false;
    }
}
