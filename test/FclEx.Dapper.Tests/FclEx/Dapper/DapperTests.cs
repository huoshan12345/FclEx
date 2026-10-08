namespace FclEx.Dapper;

[CollectionDefinition(nameof(DapperTestsCollection))]
public class DapperTestsCollection : ICollectionFixture<DapperTestsFixture>;

[Collection(nameof(DapperTestsCollection))]
public class DapperTests(DapperTestsFixture fixture) : DatabaseTests
{
    public DapperTestsFixture Fixture { get; } = fixture;

    public static readonly int[] Counts = [1, 5];

    public static readonly TheoryData<DbDriver, string?, int> BulkInsertTestCases =
    (
        from pair in DatabaseTestSettings.GetDriverSchemaCases(Schemas)
        from z in Counts
        select (pair.Driver, pair.Schema, z)
    ).ToTheoryData();

    public static readonly TheoryData<DbDriver, string?> MySqlSchemaCases = new[] { DbDriver.MySqlConnector, DbDriver.MySql }
        .CrossJoin(Schemas)
        .ToTheoryData();

    public static readonly TheoryData<string?> SchemaCases = Schemas.ToTheoryData();
    public static readonly TheoryData<DbDriver, string?> DbSchemaTestCases = DatabaseTestSettings.GetDriverSchemaCases(Schemas).ToTheoryData();
}
