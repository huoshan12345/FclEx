namespace FclEx.Utils;

public static class OperationIOPairs
{
    public static OperationIOPairs<TInput, TOutput> Create<TInput, TOutput>(
        IReadOnlyList<IOPair<TInput, TOutput>> success,
        IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> failure)
        => new(success, failure);
}

public readonly partial record struct OperationIOPairs<TInput, TOutput>(
    IReadOnlyList<IOPair<TInput, TOutput>> Success,
    IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failure)
{
    public static implicit operator OperationIOPairs<TInput, TOutput>((
        IReadOnlyList<IOPair<TInput, TOutput>> Success,
        IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failure) tuple)
    {
        return new(tuple.Success, tuple.Failure);
    }
}

#if NET7_0_OR_GREATER
partial record struct OperationIOPairs<TInput, TOutput>
    : IAdditionOperators<
        OperationIOPairs<TInput, TOutput>,
        OperationIOPairs<TInput, TOutput>,
        OperationIOPairs<TInput, TOutput>>
{
    public static OperationIOPairs<TInput, TOutput> operator +(OperationIOPairs<TInput, TOutput> left, OperationIOPairs<TInput, TOutput> right)
    {
        var success = left.Success.Concat(right.Success).ToArray();
        var failure = left.Failure.Concat(right.Failure).ToArray();
        return new(success, failure);
    }
}
#endif