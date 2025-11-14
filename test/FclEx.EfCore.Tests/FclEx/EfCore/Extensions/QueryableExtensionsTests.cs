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

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteUpdateAsync_Updates_Single_Property(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbProviderType);

        var updated = await context.EntityHasStates
            .Where(p => p.Id == entity.Id)
            .ExecuteUpdateAsync(new Dictionary<string, object?>
            {
                [nameof(entity.Name)] = "Updated Name",
            });

        Assert.Equal(1, updated);

        var result = await context.EntityHasStates.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteUpdateAsync_Updates_Multiple_Properties(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbProviderType);

        var updated = await context.EntityHasStates
            .Where(p => p.Id == entity.Id)
            .ExecuteUpdateAsync(new Dictionary<string, object?>
            {
                [nameof(entity.Name)] = "Updated Name",
                [nameof(entity.IsDeleted)] = true,
            });

        Assert.Equal(1, updated);

        var result = await context.EntityHasStates.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.True(result.IsDeleted);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteUpdateAsync_Throws_When_Property_Not_Found(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.EntityHasStates.ExecuteUpdateAsync(new Dictionary<string, object?>
            {
                ["NotARealProperty"] = 123
            })
        );
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteUpdateAsync_No_Updates_When_FieldValues_Empty(DbProviderType dbProviderType)
    {
        var entity = await CreateEntityHasStatesAsync(dbProviderType);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbProviderType);

        var updated = await context.EntityHasStates
            .Where(p => p.Id == entity.Id)
            .ExecuteUpdateAsync(new Dictionary<string, object?>());

        Assert.Equal(0, updated);

        var result = await context.EntityHasStates.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(DbTestCases))]
    public async Task ExecuteUpdateAsync_Cancels_When_Token_Requested(DbProviderType dbProviderType)
    {
        await using var context = Fixture.CreateDbContext(dbProviderType);
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // OperationCanceledException or TaskCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            context.EntityHasStates.ExecuteUpdateAsync(
                new Dictionary<string, object?> { [nameof(EntityHasStates.Name)] = "test" },
                cts.Token
            )
        );
    }
}