namespace FclEx.Dapper;

[CollectionDefinition(nameof(DapperTestsCollection))]
public class DapperTestsCollection : ICollectionFixture<DapperFixture>;


[Collection(nameof(DapperTestsCollection))]
public class DapperTests(DapperFixture fixture) : DatabaseTests
{
    public DapperFixture Fixture { get; } = fixture;
}