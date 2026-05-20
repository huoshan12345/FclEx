namespace FclEx.Dapper;

[CollectionDefinition(nameof(DapperTestsCollection))]
public class DapperTestsCollection : ICollectionFixture<DapperFixture>;

[EnableParallelization]
[Collection(nameof(DapperTestsCollection))]
public class DapperTests(DapperFixture fixture) : DatabaseTests
{
    public DapperFixture Fixture { get; } = fixture;

    [return: NotNullIfNotNull(nameof(str))]
    public static string? WithAssemblyInfo(string? str, char separator = '_')
    {
        return GlobalFixture.WithAssemblyInfo(str, typeof(DapperTests).Assembly, separator);
    }

    public static readonly int[] Counts = [1, 5];

    public static readonly string?[] Schemas = SchemaNames.Select(m => WithAssemblyInfo(m)).ToArray();

    public static readonly TheoryData<DbDriver, string?, int> BulkInsertTestCases =
    (
        from x in DbDrivers
        from y in Schemas
        from z in Counts
        select (x, y, z)
    ).ToTheoryData();

    public static readonly TheoryData<DbDriver, string?> MySqlSchemaCases = new[] { DbDriver.MySqlConnector, DbDriver.MySql }
        .SelectMany(Schemas)
        .ToTheoryData();

    public static readonly TheoryData<string?> SchemaCases = Schemas.ToTheoryData();
    public static readonly TheoryData<DbDriver, string?> DbSchemaTestCases = DbDrivers.CrossJoin(Schemas).ToTheoryData();
}