namespace FclEx.EfCore.Extensions;

public class QueryableExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    public static readonly IEnumerable<object?[]> ContainsAnyTestCases = DbTestCases
        .SelectMany([true, false])
        .Select(x => x.Left.Append(x.Right).ToArray());

    [Theory]
    [MemberData(nameof(ContainsAnyTestCases))]
    public async Task ContainsAny_Test(DbProviderType dbProviderType, bool containsPercentSign)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        await context.EntityWithAutoKey.ExecuteDeleteAsync();

        var list = Enumerable.Range(1, 9)
            .Select(CreateName)
            .Select(m => new EntityWithAutoKey
            {
                Name = m,
                Value = 1,
            });
        context.EntityWithAutoKey.AddRange(list);
        await context.SaveChangesAsync();

        var keywords = new[] { CreateKeyword(4), CreateKeyword(6) };

        var result = await context.EntityWithAutoKey
            .ContainsAny(m => m.Name, keywords)
            .ToListAsync();

        Assert.Equal(2, result.Count);
        Assert.True(keywords.All(m => result.Any(x => x.Name!.Contains(m))));

        string CreateKeyword(int number)
        {
            return StringBuilderHelper.Build(m =>
            {
                m.Append(number);
                if (containsPercentSign)
                {
                    m.Append('%');
                }
                m.Append(number);
            });
        }

        string CreateName(int number)
        {
            return StringBuilderHelper.Build(m =>
            {
                m.Append("prefix_");
                m.Append(CreateKeyword(number));
                m.Append("_postfix");
            });
        }
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task GetAsync_ShouldReturnEntity_WhenEntityExists(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.EntityHasStates.GetAsync(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task GetAsync_ShouldReturnNull_WhenEntityDoesNotExist(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);

        var result = await context.EntityHasStates.GetAsync(0);

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task GetAsync_ShouldNotTrackEntity_WhenNoTrackingIsTrue(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.EntityHasStates.GetAsync(entity.Id, noTracking: true);

        Assert.NotNull(result);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task GetAsync_ShouldTrackEntity_WhenNoTrackingIsFalse(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var result = await context.EntityHasStates.GetAsync(entity.Id, noTracking: false);

        Assert.NotNull(result);
        Assert.Single(context.ChangeTracker.Entries());
    }
}