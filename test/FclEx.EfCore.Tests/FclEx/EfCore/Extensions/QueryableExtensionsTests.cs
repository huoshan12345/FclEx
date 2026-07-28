namespace FclEx.EfCore.Extensions;

public class QueryableExtensionsTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
    private sealed class NullableKeyEntity : IHasId<string?>
    {
        public string? Id { get; set; }
    }

    public static readonly TheoryData<DbDriver, bool> ContainsAnyTestCases = DbDrivers
        .CrossJoin([true, false])
        .ToTheoryData();

    [Theory]
    [MemberData(nameof(ContainsAnyTestCases))]
    public async Task ContainsAny_Test(DbDriver dbDriver, bool containsPercentSign)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        var value = dbDriver.ToInt();
        var prefix = Guid.NewGuid().ToString();
        var list = Enumerable.Range(1, 9)
            .Select(CreateName)
            .Select(m => new EntityWithAutoKey
            {
                Name = prefix + m,
                Value = value,
            })
            .ToArray();

        context.EntityWithAutoKey.AddRange(list);
        await context.SaveChangesAsync();

        var keywordValues = new[] { "x", "y", "z" }; // do not use characters that may be in a guid.
        var keywords = keywordValues.Select(CreateKeyword).ToArray();

        var escapeEscapeCharacter = dbDriver is DbDriver.MySql;
        var query = context.EntityWithAutoKey
            .Where(m => m.Value == value)
            .ContainsAny(m => m.Name, keywords, escapeEscapeCharacter: escapeEscapeCharacter);

        Output?.WriteLine(query.ToQueryString());

        var result = await query.ToListAsync();

        Assert.Equal(list.Count(m => m.Name?.ContainsAny(keywords) == true && m.Value == value), result.Count);
        Assert.True(result.All(m => m.Name?.ContainsAny(keywords) == true && m.Value == value));

        string CreateKeyword(string raw)
        {
            return StringBuilderHelper.Build(m =>
            {
                m.Append(raw);
                if (containsPercentSign)
                {
                    m.Append('%');
                }
                m.Append(raw);
            });
        }

        string CreateName(int number)
        {
            return StringBuilderHelper.Build(m =>
            {
                m.Append("_prefix_");
                m.Append(CreateKeyword(number.ToString()));
                m.Append("_postfix");
            });
        }
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ContainsAny_ShouldTreatUnderscoreAsLiteral(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        var prefix = Guid.NewGuid().ToString();
        var literalMatch = new EntityWithAutoKey { Name = $"{prefix}_a_b" };
        var wildcardMatch = new EntityWithAutoKey { Name = $"{prefix}_axb" };
        context.EntityWithAutoKey.AddRange(literalMatch, wildcardMatch);
        await context.SaveChangesAsync();

        var escapeEscapeCharacter = dbDriver is DbDriver.MySql;
        var result = await context.EntityWithAutoKey
            .Where(e => e.Name!.StartsWith(prefix))
            .ContainsAny(e => e.Name, ["a_b"], escapeEscapeCharacter: escapeEscapeCharacter)
            .ToListAsync();

        Assert.Collection(result, entity => Assert.Equal(literalMatch.Id, entity.Id));
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ContainsAny_ShouldReturnNoMatches_WhenKeywordsAreEmpty(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        var prefix = Guid.NewGuid().ToString();
        context.EntityWithAutoKey.Add(new EntityWithAutoKey { Name = prefix });
        await context.SaveChangesAsync();

        var result = await context.EntityWithAutoKey
            .Where(e => e.Name == prefix)
            .ContainsAny(e => e.Name, [])
            .ToListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_ShouldThrow_WhenIdIsNull()
    {
        var query = Array.Empty<NullableKeyEntity>().AsQueryable();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => query.GetAsync<NullableKeyEntity, string?>(null));
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task GetAsync_ShouldReturnEntity_WhenEntityExists(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.EntityHasStates.GetAsync(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task GetAsync_ShouldReturnNull_WhenEntityDoesNotExist(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        var result = await context.EntityHasStates.GetAsync(0);

        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task GetAsync_ShouldNotTrackEntity_WhenNoTrackingIsTrue(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.EntityHasStates.GetAsync(entity.Id, noTracking: true);

        Assert.NotNull(result);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task GetAsync_ShouldTrackEntity_WhenNoTrackingIsFalse(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbDriver);
        var result = await context.EntityHasStates.GetAsync(entity.Id, noTracking: false);

        Assert.NotNull(result);
        Assert.Single(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteUpdateAsync_Updates_Single_Property(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbDriver);

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
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteUpdateAsync_Updates_Multiple_Properties(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbDriver);

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
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteUpdateAsync_Throws_When_Property_Not_Found(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            context.EntityHasStates.ExecuteUpdateAsync(new Dictionary<string, object?>
            {
                ["NotARealProperty"] = 123
            })
        );
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteUpdateAsync_No_Updates_When_FieldValues_Empty(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);

        // need to create a new context to ensure no tracked entities.
        await using var context = Fixture.CreateDbContext(dbDriver);

        var updated = await context.EntityHasStates
            .Where(p => p.Id == entity.Id)
            .ExecuteUpdateAsync(new Dictionary<string, object?>());

        Assert.Equal(0, updated);

        var result = await context.EntityHasStates.FindAsync(entity.Id);
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteUpdateAsync_Cancels_When_Token_Requested(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
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
