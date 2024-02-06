namespace FclEx.Cooperation.NewRelic;

public class NewRelicClientTests
{
    private static readonly string AccountId = Tests.GlobalFixture.AppSettings.NewRelic.AccountId;

    private readonly ITestOutputHelper _output;

    public NewRelicClientTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task NrqlQueryAsync_Test()
    {
        var start = new DateTimeOffset(2024, 2, 6, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(1);

        var query = $"""
                     SELECT * FROM Log
                     SINCE {start.ToUnixTimeMilliseconds()} UNTIL {end.ToUnixTimeMilliseconds()}
                     """;

        var result = await NewRelicApi.NrqlQueryAsync<JObject>(AccountId, query);
        Assert.NotEmpty(result.Results);

        var time = result.Metadata.TimeWindow;
        _output.WriteLine($"{time.BeginTime} - {time.EndTime}");
        _output.WriteLine(result.Results!.First().ToString(Formatting.Indented));
    }

    [Fact]
    public async Task NrqlQueryAsync_FACET_Test()
    {
        var start = new DateTimeOffset(2024, 2, 6, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddDays(1);

        var query = $"""
                     SELECT COUNT(1) FROM Log
                     SINCE {start.ToUnixTimeMilliseconds()} UNTIL {end.ToUnixTimeMilliseconds()}
                     FACET `@l`
                     """;

        var result = await NewRelicApi.NrqlQueryAsync<JObject>(AccountId, query);
        Assert.NotEmpty(result.Results);
        Assert.NotEmpty(result.Metadata.Facets);
        Assert.Equal("@l", result.Metadata.Facets[0]);

        var time = result.Metadata.TimeWindow;
        _output.WriteLine($"{time.BeginTime} - {time.EndTime}");

        foreach (var m in result.Results)
        {
            _output.WriteLine(m.ToString(Formatting.Indented));
        }

    }
}