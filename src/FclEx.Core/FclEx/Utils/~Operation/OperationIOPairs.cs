// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
namespace FclEx.Utils;

public static class OperationIOPairs
{
    public static OperationIOPairs<TInput, TOutput> Create<TInput, TOutput>(
        IReadOnlyList<IOPair<TInput, TOutput>> succeeded,
        IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> failed)
        => new(succeeded, failed);
}

public readonly partial record struct OperationIOPairs<TInput, TOutput>(
    IReadOnlyList<IOPair<TInput, TOutput>> Succeeded,
    IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failed)
{
    public static implicit operator OperationIOPairs<TInput, TOutput>((
        IReadOnlyList<IOPair<TInput, TOutput>> Succeeded,
        IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failed) tuple)
    {
        return new(tuple.Succeeded, tuple.Failed);
    }

    public IReadOnlyList<IOPair<TInput, TOutput>> Succeeded { get; init; } = Succeeded ?? [];
    public IReadOnlyList<IOPair<TInput, OperationResult<TOutput>>> Failed { get; init; } = Failed ?? [];
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
#endif
