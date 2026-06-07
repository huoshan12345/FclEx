namespace FclEx.Utils;

public class OperationIOPairsTests
{
    [Fact]
    public void Default_ReturnsEmptyLists()
    {
        var pairs = default(OperationIOPairs<string, int>);

        Assert.Empty(pairs.Succeeded);
        Assert.Empty(pairs.Failed);
    }

    [Fact]
    public void Create_StoresSucceededAndFailedPairs()
    {
        IOPair<string, int>[] succeeded = [("a", 1)];
        IOPair<string, OperationResult<int>>[] failed = [("b", Operation.Error<int>("x"))];

        var pairs = OperationIOPairs.Create(succeeded, failed);

        Assert.Same(succeeded, pairs.Succeeded);
        Assert.Same(failed, pairs.Failed);
    }

    [Fact]
    public void Addition_ConcatenatesSucceededAndFailedPairs()
    {
        OperationIOPairs<string, int> left = (
            [("a", 1)],
            [("b", Operation.Error<int>("x"))]);
        OperationIOPairs<string, int> right = (
            [("c", 2)],
            [("d", Operation.Error<int>("y"))]);

        var pairs = left + right;

        Assert.Collection(pairs.Succeeded,
            x => Assert.Equal(("a", 1), (x.Input, x.Output)),
            x => Assert.Equal(("c", 2), (x.Input, x.Output)));
        Assert.Collection(pairs.Failed,
            x => Assert.Equal("b", x.Input),
            x => Assert.Equal("d", x.Input));
    }
}
