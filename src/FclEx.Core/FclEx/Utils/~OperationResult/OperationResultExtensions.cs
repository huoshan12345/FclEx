namespace FclEx.Utils;

public static partial class OperationResultExtensions
{
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? ex, out TimeSpan elapsed)
    {
        success = result.Success;
        elapsed = result.Elapsed;
        ex = result.Exception;
    }

    [SuppressMessage("ReSharper", "UseDeconstructionOnParameter")]
    public static void Deconstruct(this OperationResult result, out bool success, out Exception? ex)
    {
        success = result.Success;
        ex = result.Exception;
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static OperationResult Merge(this IEnumerable<OperationResult> enumerable)
    {
        Check.NotNull(enumerable);

        var time = enumerable.EmptyIfNull().Sum(m => m.Elapsed);
        var exceptions = enumerable.EmptyIfNull().Select(m => m.Exception).NotNull().ToList();
        return exceptions.Count switch
        {
            0 => Operation.Success(time),
            1 => Operation.Error(exceptions[0], time),
            _ => Operation.Error(new AggregateException(exceptions), time)
        };
    }

    public static bool IsObjectError<T>(this OperationResult<T> result, Func<T, bool> condition) where T : notnull
    {
        return result.Error && result.Exception.IsObjectException(condition);
    }

    public static bool IsObjectError<T>(this IOperationResult result, Func<T, bool> condition) where T : notnull
    {
        return result.Error && result.Exception.IsObjectException(condition);
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

    public static bool IsCanceled(this IOperationResult result)
    {
        return result.Error && result.Exception.IsCanceled();
    }
}