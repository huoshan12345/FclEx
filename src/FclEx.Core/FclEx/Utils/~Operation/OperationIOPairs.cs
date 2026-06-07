// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
namespace FclEx.Utils;

public static class OperationIOPairs
{
    /// <summary>
    /// Creates input/output pairs partitioned by operation success.
    /// </summary>
    /// <typeparam name="TInput">The input value type.</typeparam>
    /// <typeparam name="TOutput">The output value type.</typeparam>
    /// <param name="succeeded">Pairs whose operations succeeded.</param>
    /// <param name="failed">Pairs whose operations failed, retaining the failed operation result.</param>
    /// <returns>A partitioned pair collection.</returns>
    public static OperationIOPairs<TInput, TOutput> Create<TInput, TOutput>(
        IReadOnlyList<IOPair<TInput, TOutput>> succeeded,
        IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> failed)
        => new(succeeded, failed);
}

public readonly partial record struct OperationIOPairs<TInput, TOutput>(
    IReadOnlyList<IOPair<TInput, TOutput>> Succeeded,
    IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failed)
#if NET7_0_OR_GREATER
    : IAdditionOperators<
        OperationIOPairs<TInput, TOutput>,
        OperationIOPairs<TInput, TOutput>,
        OperationIOPairs<TInput, TOutput>>
#endif
{
    /// <summary>
    /// Converts a tuple of succeeded and failed pair lists to an <see cref="OperationIOPairs{TInput, TOutput}"/>.
    /// </summary>
    /// <param name="tuple">The succeeded and failed pair lists.</param>
    public static implicit operator OperationIOPairs<TInput, TOutput>((
        IReadOnlyList<IOPair<TInput, TOutput>> Succeeded,
        IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failed) tuple)
    {
        return new(tuple.Succeeded, tuple.Failed);
    }

    public IReadOnlyList<IOPair<TInput, TOutput>> Succeeded
    {
        get => field ?? [];
        init;
    } = Succeeded;

    public IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failed
    {
        get => field ?? [];
        init;
    } = Failed;

    /// <summary>
    /// Concatenates the succeeded and failed pair lists from two partitioned collections.
    /// </summary>
    /// <param name="left">The left collection.</param>
    /// <param name="right">The right collection.</param>
    /// <returns>A collection containing pairs from both operands.</returns>
    public static OperationIOPairs<TInput, TOutput> operator +(OperationIOPairs<TInput, TOutput> left, OperationIOPairs<TInput, TOutput> right)
    {
        var succeeded = Concat(left.Succeeded, right.Succeeded);
        var failed = Concat(left.Failed, right.Failed);
        return new(succeeded, failed);

        static IReadOnlyList<T> Concat<T>(IReadOnlyList<T>? first, IReadOnlyList<T>? second)
        {
            if (first.IsNullOrEmpty())
                return second ?? [];
            if (second.IsNullOrEmpty())
                return first ?? [];

            return first.Concat(second).ToArray();
        }
    }
}
