using FclEx.Dapper;

namespace FclEx.Databases;

public class TruncateTestSessionTests(DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task Sessions_IsolateWholeTableOperations(DbDriver driver, string? schema)
    {
        Assert.SkipWhen(driver == DbDriver.MySql, "MySql.Data asynchronous timeout cleanup can hang.");
        using var first = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        using var second = await Fixture.TruncateTables.CreateSessionAsync(driver, schema, [typeof(EntityWithAutoKey)], CancellationToken);
        var firstTable = first.GetQualifiedTableName(typeof(EntityWithAutoKey));
        var secondTable = second.GetQualifiedTableName(typeof(EntityWithAutoKey));
        try
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {firstTable}", cancellationToken: CancellationToken);
            await second.Connection.ExecuteAsync($"DELETE FROM {secondTable}", cancellationToken: CancellationToken);
            await first.Connection.InsertAsync(new EntityWithAutoKey { Name = "first", Value = 1 }, first.Schema,
                commandOptions: first.CommandOptions, cancellationToken: CancellationToken);
            await second.Connection.InsertAsync(new EntityWithAutoKey { Name = "second", Value = 2 }, second.Schema,
                commandOptions: second.CommandOptions, cancellationToken: CancellationToken);
            await first.Connection.TruncateAsync<EntityWithAutoKey>(first.Schema, first.CommandOptions, CancellationToken);
            var firstCount = await first.Connection.QuerySingleAsync<int>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {firstTable}", cancellationToken: CancellationToken));
            var secondCount = await second.Connection.QuerySingleAsync<int>(new CommandDefinition(
                $"SELECT COUNT(*) FROM {secondTable}", cancellationToken: CancellationToken));
            Assert.Equal(0, firstCount);
            Assert.Equal(1, secondCount);
        }
        finally
        {
            await first.Connection.ExecuteAsync($"DELETE FROM {firstTable}", cancellationToken: CancellationToken);
            await second.Connection.ExecuteAsync($"DELETE FROM {secondTable}", cancellationToken: CancellationToken);
        }
    }
}
