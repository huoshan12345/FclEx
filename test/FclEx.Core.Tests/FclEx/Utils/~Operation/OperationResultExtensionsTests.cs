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
}
