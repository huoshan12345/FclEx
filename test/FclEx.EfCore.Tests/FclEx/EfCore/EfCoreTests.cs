using System.Diagnostics.CodeAnalysis;
using FclEx.Dapper;

namespace FclEx.EfCore;

[CollectionDefinition(nameof(EfCoreTestsCollection))]
public class EfCoreTestsCollection : ICollectionFixture<EfCoreFixture>;

[EnableParallelization]
[Collection(nameof(EfCoreTestsCollection))]
public class EfCoreTests(EfCoreFixture fixture) : DatabaseTests
{
    public static readonly string?[] Schemas = SchemaNames.Select(m => WithAssemblyInfo(m)).ToArray();
    public static readonly TheoryData<DbDriver, string?> DbSchemaTestCases = DbDrivers.CrossJoin(Schemas).ToTheoryData();

    public EfCoreFixture Fixture { get; } = fixture;

    protected async Task<EntityHasStates> CreateEntityHasStatesAsync(DbDriver dbDriver)
    {
        var entity = new EntityHasStates { Name = Guid.NewGuid().ToString() };
        await using var context = Fixture.CreateDbContext(dbDriver);
        context.EntityHasStates.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    protected async Task<EntityWithNavigation> CreateEntityWithNavigationAsync(DbDriver dbDriver)
    {
        var entity = new EntityWithNavigation
        {
            Name = Guid.NewGuid().ToString(),
            Navigation = new EntityHasStates
            {
                Name = Guid.NewGuid().ToString(),
            },
        };
        await using var context = Fixture.CreateDbContext(dbDriver);
        context.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    [return: NotNullIfNotNull(nameof(str))]
    public static string? WithAssemblyInfo(string? str, char separator = '_')
    {
        return CoreTestsFixture.WithAssemblyInfo(str, typeof(EfCoreTests).Assembly, separator);
    }
}