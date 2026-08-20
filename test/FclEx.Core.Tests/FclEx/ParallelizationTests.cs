namespace FclEx;

public class ParallelizationTests
{
    [Theory(Skip = "Run it only when needed")]
    [InlineData(2)]
    [InlineData(2.5)]
    public async Task Theory_Test(double value)
    {
        await Task.Delay(TimeSpan.FromSeconds(value), TestContext.Current.CancellationToken);
    }
}

[CollectionDefinition(nameof(ParallelizationCollection))]
public class ParallelizationCollection;

[Collection(nameof(ParallelizationCollection))]
public class ParallelizationCollectionTests
{
    [Theory(Skip = "Run it only when needed")]
    [InlineData(2)]
    [InlineData(2.5)]
    public async Task Theory_Test(double value)
    {
        await Task.Delay(TimeSpan.FromSeconds(value), TestContext.Current.CancellationToken);
    }
}
