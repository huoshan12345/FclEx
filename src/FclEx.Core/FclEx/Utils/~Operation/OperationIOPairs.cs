// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
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

    public IReadOnlyList<IOPair<TInput, TOutput>> Success { get; init; } = Success ?? [];
    public IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failure { get; init; } = Failure ?? [];
}

#if NET7_0_OR_GREATER
public readonly partial record struct OperationIOPairs<TInput, TOutput>
    : IAdditionOperators<
        OperationIOPairs<TInput, TOutput>,
        OperationIOPairs<TInput, TOutput>,
        OperationIOPairs<TInput, TOutput>>
{
    public static OperationIOPairs<TInput, TOutput> operator +(OperationIOPairs<TInput, TOutput> left, OperationIOPairs<TInput, TOutput> right)
    {
        var success = Concat(left.Success, right.Success);
        var failure = Concat(left.Failure, right.Failure);
        return new(success, failure);

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
#endif
