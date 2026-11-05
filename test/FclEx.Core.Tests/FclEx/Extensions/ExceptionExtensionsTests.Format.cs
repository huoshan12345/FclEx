namespace FclEx.Extensions;

partial class ExceptionExtensionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void BuildTree_SingleAggregateWrapper_PreservesRealParentIndexes(int shape)
    {
        Exception exception = shape switch
        {
            0 => new AggregateException(new InvalidOperationException("leaf")),
            1 => new InvalidOperationException("root", new AggregateException(new ArgumentException("leaf"))),
            2 => new AggregateException("branches", new AggregateException(new InvalidOperationException("first")), new ArgumentException("second")),
            _ => new AggregateException("keep this wrapper", new InvalidOperationException("leaf"))
        };
        (int Index, int ParentIndex)[] expected = shape switch
        {
            0 => [(0, -1)],
            2 => [(0, -1), (1, 0), (2, 0)],
            _ => [(0, -1), (1, 0)]
        };

        var tree = exception.BuildTree();

        Assert.Equal(expected, tree.Nodes.Select(info => (info.Index, info.ParentIndex)));
    }

    [Fact]
    public async Task BuildTree_Complex_Test()
    {
        try
        {
            await ExceptionCreator.Run();
        }
        catch (Exception ex)
        {
            var (infos, _) = ex.BuildTree();

            foreach (var info in infos)
            {
                foreach (var line in info.StackTraceLines)
                {
                    Assert.True(ExceptionExtensions.ExcludeStackTracePrefixes.All(m => line.StartsWith(m) == false));
                }
            }
        }
    }

    [Fact]
    public void GetInfo_Test()
    {
        var text = File.ReadAllText(Path.Combine("TestData", "StackTrace.txt"));
        var ex = new Exception().SetStackTrace(text);

        var index = 0;
        var info = ex.GetInfo(ref index, -1, null);

        foreach (var line in info.StackTraceLines)
        {
            Assert.True(ExceptionExtensions.ExcludeStackTracePrefixes.All(m => line.StartsWith(m) == false));
        }
    }
}
