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
}
