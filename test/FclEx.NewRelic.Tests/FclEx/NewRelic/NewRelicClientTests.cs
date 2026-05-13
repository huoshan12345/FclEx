namespace FclEx.NewRelic;

public class NewRelicClientTests
{
    private static readonly string AccountId = NewRelicFixture.NewRelicConfig.AccountId;

    private readonly ITestOutputHelper _output;

    public NewRelicClientTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [LocalOnlyFact]
    public async Task NrqlQueryAsync_Test()
    {
        var start = new DateTimeOffset(2024, 2, 6, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(1);

        var query = $"""
                     SELECT * FROM Log
                     SINCE {start.ToUnixTimeMilliseconds()} UNTIL {end.ToUnixTimeMilliseconds()}
                     """;

        var result = await NewRelicApi.NrqlQueryAsync(AccountId, query);
        var time = result.Metadata.TimeWindow;
        _output.WriteLine($"{time.BeginTime} - {time.EndTime}");

        if (result.Results is [var resultItem, ..])
        {
            _output.WriteLine(resultItem.ToJsonString(new() { WriteIndented = true }));
        }
    }

    [LocalOnlyFact]
    public async Task NrqlQueryAsync_Facet_Test()
    {
        var start = new DateTimeOffset(2024, 2, 6, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(1);

        var query = $"""
                     SELECT COUNT(1) FROM Log
                     SINCE {start.ToUnixTimeMilliseconds()} UNTIL {end.ToUnixTimeMilliseconds()}
                     FACET `@l`
                     """;

        var result = await NewRelicApi.NrqlQueryAsync(AccountId, query);

        Assert.NotEmpty(result.Metadata.Facets);
        Assert.Equal("@l", result.Metadata.Facets[0]);

        var time = result.Metadata.TimeWindow;
        _output.WriteLine($"{time.BeginTime} - {time.EndTime}");

        foreach (var m in result.Results)
        {
            _output.WriteLine(m.ToJsonString(new() { WriteIndented = true }));
        }

    }
}