namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? ex, out TimeSpan elapsed)
    {
        success = result.Success;
        elapsed = result.Elapsed;
        ex = result.Exception;
    }

    public static void Deconstruct(this OperationResult result, out bool success, out Exception? ex)
    {
        success = result.Success;
        ex = result.Exception;
    }

    /// <summary>
    /// Merges multiple <see cref="IOperationResult"/> instances into a single result.
    /// </summary>
    /// <param name="enumerable">The collection of <see cref="IOperationResult"/> instances to merge.</param>
    /// <returns>
    /// A new <see cref="OperationResult"/> representing the merged results.<br/>
    /// If all results are successful, it returns a success result with the total elapsed time.<br/>
    /// If there is one exception, it returns an error result with that exception.
    /// If multiple exceptions exist, it returns an error result with an <see cref="AggregateException"/>.
    /// </returns>
    public static OperationResult Merge(this IEnumerable<IOperationResult> enumerable)
    {
        Check.NotNull(enumerable);

        var (time, exceptions) = enumerable.Aggregate((Time: TimeSpan.Zero, List: new List<Exception>()), (seed, m) =>
        {
            var t = seed.Time + m.Elapsed;
            var e = m.Exception;
            return (t, e is null ? seed.List : seed.List.Push(e));
        });

        return exceptions.Count switch
        {
            0 => Operation.Success(time),
            1 => Operation.Error(exceptions[0], time),
            _ => Operation.Error(new AggregateException(exceptions), time)
        };
    }

    /// <summary>
    /// Determines whether the operation result represents an error that satisfies a given condition 
    /// based on the exception's associated object.
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
        return result.Exception.IsObjectException(out value) && condition(value, result.Exception);
    }

    /// <summary>
    /// Determines whether the operation result represents an error that satisfies a given condition 
    /// based on the exception's associated object, without extracting the object.
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
    /// Determines whether the operation result represents a canceled operation.
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns>
    /// <see langword="true"/> if the result is an error and its exception represents a canceled operation;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsCanceled(this IOperationResult result)
    {
        return result.Error && result.Exception.IsCanceled();
    }

    /// <summary>
    /// Checks if the operation result indicates an error and that the error is represented as a simple string message.
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns>True if the result is an error and the error is a simple string message, false otherwise.</returns>
    public static bool IsStringError(this IOperationResult result)
    {
        return result.Error && result.Exception.IsJustMessage();
    }

    /// <summary>
    /// Checks if the operation result indicates an error and that the error is *not* represented as a simple string message.
    /// This implies the error is a more complex type (e.g., an exception object with details).
    /// </summary>
    /// <param name="result">The operation result to check.</param>
    /// <returns>True if the result is an error and the error is *not* a simple string message, false otherwise.</returns>
    public static bool IsNonStringError(this IOperationResult result)
    {
        return result.Error && result.Exception.IsJustMessage() == false;
    }
}