namespace FclEx.Dapper;

partial class DbConnectionExtensionsTests
{
    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task QueryAsync_DateTimeOffsetParameter_RoundTripsInstant(DbDriver dbDriver)
    {
        using var connection = Fixture.CreateDbConnection(dbDriver, null);
        var adapter = DapperHelper.GetSqlAdapter(connection);
        var expected = new DateTimeOffset(2026, 10, 7, 12, 34, 56, TimeSpan.Zero).AddTicks(1234560);
        var placeholder = adapter.GetParameterPlaceholder("Value");
        var column = adapter.GetQuotedColumnName("Value");

        var actual = await connection.QuerySingleAsync<TimestampRow>(
            $"SELECT {placeholder} AS {column}", new { Value = expected });

        Assert.Equal(expected.UtcDateTime.Ticks, actual.Value.UtcDateTime.Ticks);
    }

    private sealed class TimestampRow
    {
        public DateTimeOffset Value { get; set; }
    }
}
