namespace FclEx.Utils;

public partial class OperationResultExtensionsTests
{
    [Fact]
    public void OperationResultExtensions_RejectNullDelegates()
    {
        var result = Operation.Success(1);

        Assert.Throws<ArgumentNullException>(() => { _ = result.MapValue<int, int>(null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = result.Then<int, int>((Func<int, OperationResult<int>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = result.ThenResult<int, int>((Func<OperationResult<int>, OperationResult<int>>)null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = result.IsSuccess(null!); });
        Assert.Throws<ArgumentNullException>(() => { _ = result.ThenWith<int, int>((Func<int, int>)null!); });
    }

    [Fact]
    public void Then_AddsElapsedFromBothResults()
    {
        var result = Operation.Success(1, TimeSpan.FromSeconds(2))
            .Then(value => Operation.Success(value + 1, TimeSpan.FromSeconds(3)));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public void ThenResult_AddsElapsedFromBothResults()
    {
        var result = Operation.Success(1, TimeSpan.FromSeconds(2))
            .ThenResult(_ => Operation.Success("x", TimeSpan.FromSeconds(3)));

        Assert.True(result.IsSuccess);
        Assert.Equal("x", result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public void ThenWith_OperationResult_AddsElapsedFromBothResults()
    {
        var result = Operation.Success("a", TimeSpan.FromSeconds(2))
            .ThenWith(value => Operation.Success(value + "b", TimeSpan.FromSeconds(3)));

        Assert.True(result.IsSuccess);
        Assert.Equal(("a", "ab"), result.Value);
        Assert.Equal(TimeSpan.FromSeconds(5), result.Elapsed);
    }

    [Fact]
    public void Flatten_OuterDefaultElapsed_PreservesInnerElapsed()
    {
        var inner = Operation.Success(1, TimeSpan.FromSeconds(2));
        var result = Operation.Success(inner).Flatten();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(2), result.Elapsed);
    }

    [Fact]
    public void Flatten_OuterElapsed_OverridesInnerElapsed()
    {
        var inner = Operation.Success(1, TimeSpan.FromSeconds(2));
        var result = Operation.Success(inner, TimeSpan.FromSeconds(3)).Flatten();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Equal(TimeSpan.FromSeconds(3), result.Elapsed);
    }

    [Fact]
    public void Flatten_OuterError_UsesOuterError()
    {
        var exception = new SimpleException("outer");
        var result = Operation.Error<OperationResult<int>>(exception, TimeSpan.FromSeconds(3)).Flatten();

        Assert.False(result.IsSuccess);
        Assert.Same(exception, result.Exception);
        Assert.Equal(TimeSpan.FromSeconds(3), result.Elapsed);
    }

    [Fact]
    public void Merge_Empty_ReturnsSuccessfulEmptyArray()
    {
        var result = Array.Empty<OperationResult<int>>().Merge();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        Assert.Equal(TimeSpan.Zero, result.Elapsed);
    }

    [Fact]
    public void Merge_MultipleErrors_ReturnsAggregateExceptionAndTotalElapsed()
    {
        var first = new SimpleException("first");
        var second = new SimpleException("second");

        var result = new[]
        {
            Operation.Success(1, TimeSpan.FromSeconds(1)),
            Operation.Error<int>(first, TimeSpan.FromSeconds(2)),
            Operation.Error<int>(second, TimeSpan.FromSeconds(3)),
        }.Merge();

        Assert.False(result.IsSuccess);
        var aggregateException = Assert.IsType<AggregateException>(result.Exception);
        Assert.Collection(aggregateException.InnerExceptions,
            x => Assert.Same(first, x),
            x => Assert.Same(second, x));
        Assert.Equal(TimeSpan.FromSeconds(6), result.Elapsed);
    }

    [Fact]
    public void ThenIf_FalseBranch_PreservesOriginalValue()
    {
        var called = false;

        var result = Operation.Success(1)
            .ThenIf(_ => false, _ =>
            {
                called = true;
                return Operation.Success(2);
            });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.False(called);
    }

    [Fact]
    public void Fallback_Success_DoesNotInvokeFallback()
    {
        var called = false;

        var result = Operation.Success(1)
            .Fallback(() =>
            {
                called = true;
                return Operation.Success(2);
            });

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.False(called);
    }
}
