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

    public static readonly TheoryData<DbDriver> ParameterizedPatternDbDriverCases = DbDrivers
        .Where(driver => driver.IsMySql() == false)
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
            return StringBuilder.Build(m =>
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
            return StringBuilder.Build(m =>
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
    public async Task ContainsAny_ShouldTreatEscapeCharacterAsLiteral(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        var prefix = Guid.NewGuid().ToString();
        var literalMatch = new EntityWithAutoKey { Name = $@"{prefix}_a\b" };
        var escapeRemovedMatch = new EntityWithAutoKey { Name = $"{prefix}_ab" };
        context.EntityWithAutoKey.AddRange(literalMatch, escapeRemovedMatch);
        await context.SaveChangesAsync();

        var escapeEscapeCharacter = dbDriver is DbDriver.MySql;
        var query = context.EntityWithAutoKey
            .Where(e => e.Name!.StartsWith(prefix))
            .ContainsAny(e => e.Name, [@"a\b"], escapeEscapeCharacter: escapeEscapeCharacter);

        Output?.WriteLine(query.ToQueryString());
        var result = await query.ToListAsync();

        Assert.Collection(result, entity => Assert.Equal(literalMatch.Id, entity.Id));
    }

    [Fact]
    public async Task ContainsAny_ShouldTreatSqlServerBracketAsLiteral()
    {
        if (DbDrivers.Contains(DbDriver.SqlServer) == false)
            return;

        await using var context = Fixture.CreateDbContext(DbDriver.SqlServer);

        var prefix = Guid.NewGuid().ToString();
        var literalMatch = new EntityWithAutoKey { Name = $"{prefix}_a[bc]d" };
        var wildcardMatch = new EntityWithAutoKey { Name = $"{prefix}_abd" };
        context.EntityWithAutoKey.AddRange(literalMatch, wildcardMatch);
        await context.SaveChangesAsync();

        var result = await context.EntityWithAutoKey
            .Where(e => e.Name!.StartsWith(prefix))
            .ContainsAny(e => e.Name, ["a[bc]d"])
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

    [Theory]
    [MemberData(nameof(ParameterizedPatternDbDriverCases))]
    public void ContainsAny_ShouldParameterizePatterns(DbDriver dbDriver)
    {
        using var context = Fixture.CreateDbContext(dbDriver);
        var keyword = $"sensitive-{Guid.NewGuid():N}";
        var escapeEscapeCharacter = dbDriver is DbDriver.MySql;

        var sql = context.EntityWithAutoKey
            .ContainsAny(e => e.Name, [keyword], escapeEscapeCharacter: escapeEscapeCharacter)
            .ToQueryString();

        Output?.WriteLine(sql);
        Assert.Matches(@"LIKE\s+@\w+", sql);
        Assert.DoesNotContain($"LIKE '%{keyword}%'", sql);
        Assert.DoesNotContain($"LIKE N'%{keyword}%'", sql);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenIdIsNull()
    {
        var query = Array.Empty<NullableKeyEntity>().AsQueryable();
        var result = await query.GetAsync<NullableKeyEntity, string?>(null);
        Assert.Null(result);
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

    [Fact]
    public void StateFilters_ApplyExpectedPredicates()
    {
        var active = new EntityHasStates { Id = 1 };
        var deleted = new EntityHasStates { Id = 2, IsDeleted = true };
        var disabled = new EntityHasStates { Id = 3, IsDisabled = true };
        var query = new[] { active, deleted, disabled }.AsQueryable();

        Assert.Equal([active, disabled], query.NotDeleted().ToArray());
        Assert.Equal([active, deleted], query.Enabled().ToArray());
        Assert.Equal([active], query.Valid().ToArray());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ToPagedListAsync_ShouldNotTrackEntities_WhenNoTrackingIsTrue(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        await using var context = Fixture.CreateDbContext(dbDriver);

        var result = await context.EntityHasStates
            .Where(e => e.Id == entity.Id)
            .ToPagedListAsync(pageSize: 10, pageIndex: 0, noTracking: true);

        Assert.Single(result.Items);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ToPagedListAsync_ShouldTrackEntities_WhenNoTrackingIsFalse(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        await using var context = Fixture.CreateDbContext(dbDriver);

        var result = await context.EntityHasStates
            .Where(e => e.Id == entity.Id)
            .ToPagedListAsync(pageSize: 10, pageIndex: 0, noTracking: false);

        Assert.Single(result.Items);
        Assert.Single(context.ChangeTracker.Entries<EntityHasStates>());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ToPagedListAsync_FuncSelector_ShouldProjectAfterMaterialization(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        await using var context = Fixture.CreateDbContext(dbDriver);
        Func<EntityHasStates, string> selector = value => $"{value.Id}:{value.Name}";

        var result = await context.EntityHasStates
            .Where(value => value.Id == entity.Id)
            .ToPagedListAsync(pageSize: 10, pageIndex: 0, selector);

        Assert.Equal([$"{entity.Id}:{entity.Name}"], result.Items);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ToPagedListAsync_ExpressionSelector_ShouldProjectInDatabase(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        await using var context = Fixture.CreateDbContext(dbDriver);

        var result = await context.EntityHasStates
            .Where(e => e.Id == entity.Id)
            .ToPagedListAsync(
                e => e.Name,
                pageSize: 10,
                pageIndex: 0,
                noTracking: false);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal([entity.Name], result.Items);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ToPagedListAsync_ExpressionSelector_ShouldReturnEmptyPage(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);

        var result = await context.EntityHasStates
            .Where(e => false)
            .ToPagedListAsync(e => e.Name, pageSize: 10, pageIndex: 0);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ToPagedListAsync_ShouldRejectInvalidPagingBeforeQuery(DbDriver dbDriver)
    {
        await using var context = Fixture.CreateDbContext(dbDriver);
        var query = context.EntityHasStates.Where(e => false);

        var pageSizeException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            query.ToPagedListAsync(pageSize: 0, pageIndex: 0));
        var offsetException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            query.ToPagedListAsync(pageSize: 2, pageIndex: int.MaxValue));
        var projectedOffsetException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            query.ToPagedListAsync(e => e.Name, pageSize: 2, pageIndex: int.MaxValue));

        Assert.Equal("pageSize", pageSizeException.ParamName);
        Assert.Equal("pageIndex", offsetException.ParamName);
        Assert.Equal("pageIndex", projectedOffsetException.ParamName);
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
    public async Task ExecuteSoftDeleteAsync_ShouldNotRewriteDeletedEntity(DbDriver dbDriver)
    {
        var entity = await CreateEntityHasStatesAsync(dbDriver);
        await using var context = Fixture.CreateDbContext(dbDriver);
        var query = context.EntityHasStates.Where(e => e.Id == entity.Id);

        var firstUpdateCount = await query.ExecuteSoftDeleteAsync();
        var firstDeletedAt = await context.EntityHasStates
            .Where(e => e.Id == entity.Id)
            .Select(e => e.DeletedAt)
            .SingleAsync();
        var secondUpdateCount = await query.ExecuteSoftDeleteAsync();
        var secondDeletedAt = await context.EntityHasStates
            .Where(e => e.Id == entity.Id)
            .Select(e => e.DeletedAt)
            .SingleAsync();

        Assert.Equal(1, firstUpdateCount);
        Assert.Equal(0, secondUpdateCount);
        Assert.NotEqual(default, firstDeletedAt);
        Assert.Equal(firstDeletedAt, secondDeletedAt);
    }

    [Theory]
    [MemberData(nameof(DbDriverCases))]
    public async Task ExecuteSoftDeleteAsync_PhysicallyDeletesNonSoftDeletableEntity(DbDriver dbDriver)
    {
        var entity = new EntityWithAutoKey { Name = Guid.NewGuid().ToString() };
        await using var context = Fixture.CreateDbContext(dbDriver);
        context.EntityWithAutoKey.Add(entity);
        await context.SaveChangesAsync();
        context.Entry(entity).State = EntityState.Detached;

        var affected = await context.EntityWithAutoKey
            .Where(value => value.Id == entity.Id)
            .ExecuteSoftDeleteAsync();

        Assert.Equal(1, affected);
        Assert.Null(await context.EntityWithAutoKey.FindAsync(entity.Id));
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

    [Fact]
    public void ExecuteUpdateAsync_Throws_WhenNullIsAssignedToNonNullableProperty()
    {
        using var context = new UpdateContext();

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            _ = context.Entities.ExecuteUpdateAsync(new Dictionary<string, object?>
            {
                [nameof(UpdateEntity.Value)] = null,
            });
        });

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void ExecuteUpdateAsync_AllowsNullForNullableProperty()
    {
        using var context = new UpdateContext();

        var exception = Record.Exception(() => QueryableExtensions.BuildUpdateBody(
            typeof(UpdateEntity),
            new Dictionary<string, object?> { [nameof(UpdateEntity.NullableValue)] = null }));

        Assert.Null(exception);
    }

    [Fact]
    public void ExecuteUpdateAsync_ThrowsArgumentNullException_ForNullArguments()
    {
        using var context = new UpdateContext();
        IQueryable<UpdateEntity> nullQuery = null!;
        IReadOnlyDictionary<string, object?> nullFieldValues = null!;

        var queryException = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = nullQuery.ExecuteUpdateAsync(new Dictionary<string, object?>());
        });
        var valuesException = Assert.Throws<ArgumentNullException>(() =>
        {
            _ = context.Entities.ExecuteUpdateAsync(nullFieldValues);
        });

        Assert.Equal("query", queryException.ParamName);
        Assert.Equal("fieldValues", valuesException.ParamName);
    }

    private sealed class UpdateContext : DbContext
    {
        public DbSet<UpdateEntity> Entities => Set<UpdateEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=:memory:");
        }
    }

    private sealed class UpdateEntity
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public int? NullableValue { get; set; }
    }
}
