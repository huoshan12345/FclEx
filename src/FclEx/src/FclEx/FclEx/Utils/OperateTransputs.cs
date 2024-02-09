using System.Numerics;

namespace FclEx.Utils;

public static class OperateTransputs
{
    public static OperateTransputs<TInput, TOutput> Create<TInput, TOutput>(
        IReadOnlyList<Transput<TInput, TOutput>> success,
        IReadOnlyList<Transput<TInput, OperateResult<TOutput>>> failure)
        => new(success, failure);
}

public readonly partial record struct OperateTransputs<TInput, TOutput>(
    IReadOnlyList<Transput<TInput, TOutput>> Success,
    IReadOnlyList<Transput<TInput, OperateResult<TOutput>>> Failure)
{
    public static implicit operator OperateTransputs<TInput, TOutput>((
        IReadOnlyList<Transput<TInput, TOutput>> Success,
        IReadOnlyList<Transput<TInput, OperateResult<TOutput>>> Failure) tuple)
    {
        return new(tuple.Success, tuple.Failure);
    }
}

#if NET7_0_OR_GREATER
partial record struct OperateTransputs<TInput, TOutput>
    : IAdditionOperators<
        OperateTransputs<TInput, TOutput>,
        OperateTransputs<TInput, TOutput>,
        OperateTransputs<TInput, TOutput>>
{
    public static OperateTransputs<TInput, TOutput> operator +(OperateTransputs<TInput, TOutput> left, OperateTransputs<TInput, TOutput> right)
    {
        var success = left.Success.Concat(right.Success).ToArray();
        var failure = left.Failure.Concat(right.Failure).ToArray();
        return new(success, failure);
    }
}
#endif