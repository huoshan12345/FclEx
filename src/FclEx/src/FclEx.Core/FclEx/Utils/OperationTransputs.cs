namespace FclEx.Utils;

public static class OperationTransputs
{
    public static OperationTransputs<TInput, TOutput> Create<TInput, TOutput>(
        IReadOnlyList<Transput<TInput, TOutput>> success,
        IReadOnlyList<Transput<TInput, OperationResult<TOutput>>> failure)
        => new(success, failure);
}

public readonly partial record struct OperationTransputs<TInput, TOutput>(
    IReadOnlyList<Transput<TInput, TOutput>> Success,
    IReadOnlyList<Transput<TInput, OperationResult<TOutput>>> Failure)
{
    public static implicit operator OperationTransputs<TInput, TOutput>((
        IReadOnlyList<Transput<TInput, TOutput>> Success,
        IReadOnlyList<Transput<TInput, OperationResult<TOutput>>> Failure) tuple)
    {
        return new(tuple.Success, tuple.Failure);
    }
}

#if NET7_0_OR_GREATER
partial record struct OperationTransputs<TInput, TOutput>
    : IAdditionOperators<
        OperationTransputs<TInput, TOutput>,
        OperationTransputs<TInput, TOutput>,
        OperationTransputs<TInput, TOutput>>
{
    public static OperationTransputs<TInput, TOutput> operator +(OperationTransputs<TInput, TOutput> left, OperationTransputs<TInput, TOutput> right)
    {
        var success = left.Success.Concat(right.Success).ToArray();
        var failure = left.Failure.Concat(right.Failure).ToArray();
        return new(success, failure);
    }
}
#endif