namespace FclEx.Dapper;

public partial class DbConnectionExtensionsTests(ITestOutputHelper output, DapperFixture fixture) : DapperTests(fixture)
{
    public static readonly int[] Counts = [0, 1, 5];
    public static readonly IEnumerable<object[]> BulkInsertTestCases =
        from x in DatabaseTypes
        from y in Schemas
        from z in Counts
        select new object[] { x, y, z };

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithAutoKey_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var entity = new EntityWithAutoKey
        {
            Value = 100,
            Name = Guid.NewGuid().ToString(),
        };
        var id = (long?)await db.Database.GetDbConnection().InsertAsync(entity, db.Schema);
        var e = await db.EntityWithAutoKey.Where(m => m.Name == entity.Name).FirstOrDefaultAsync();
        Assert.NotNull(e);
        Assert.Equal(entity.Value, e.Value);
        Assert.Equal(id, e.Id);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithAutoKey_IncludeAutoKey_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);
        var maxId = await db.EntityWithAutoKey.MaxAsync(m => (int?)m.Id);

        var entity = new EntityWithAutoKey
        {
            Id = maxId.Get(1) + 50,
            Value = 100,
            Name = Guid.NewGuid().ToString(),
        };
        await db.Database.GetDbConnection().InsertAsync(entity, db.Schema, includeAutoKey: true);

        var e = await db.EntityWithAutoKey
            .AsNoTracking()
            .Where(m => m.Id == entity.Id)
            .FirstOrDefaultAsync();

        Assert.NotNull(e);
        Assert.Equal(entity.Value, e.Value);
        Assert.Equal(entity.Id, e.Id);

        await db.EntityWithAutoKey.Where(m => m.Id == entity.Id).ExecuteDeleteAsync();
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithGuidKey_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var entity = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 100,
            Order = null,
        };
        var value = await db.Database.GetDbConnection().InsertAsync(entity, db.Schema);
        Assert.Null(value);
        var e = await db.EntityWithGuidKey.Where(m => m.Id == entity.Id).FirstAsync();
        Assert.Equal(entity.Value, e.Value);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithoutKey_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var entity = new EntityWithoutKey
        {
            Name = Guid.NewGuid().ToString(),
            Value = 1
        };
        await db.Database.GetDbConnection().InsertAsync(entity, db.Schema);
        var e = await db.EntityWithoutKey.Where(m => m.Name == entity.Name).FirstAsync();
        Assert.Equal(entity.Value, e.Value);
    }

    [Theory]
    [MemberData(nameof(BulkInsertTestCases))]
    public async Task BulkInsertAsync_EntityWithAutoKey_Test(DbProviderType dbProviderType, string schema, int count)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var entities = Enumerable.Range(1, count).Select(m => new EntityWithAutoKey
        {
            Value = m,
            Name = Guid.NewGuid().ToString(),
        }).ToArray();

        var rows = await db.Database.GetDbConnection().BulkInsertAsync(entities, db.Schema);
        Assert.Equal(count, rows);

        foreach (var entity in entities)
        {
            var e = await db.EntityWithAutoKey.Where(m => m.Name == entity.Name).FirstAsync();
            Assert.Equal(e.Value, entity.Value);
        }
    }

    [Theory]
    [MemberData(nameof(BulkInsertTestCases))]
    public async Task BulkInsertAsync_EntityWithAutoKey_IncludeAutoKey_Test(DbProviderType dbProviderType, string schema, int count)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var maxId = await db.EntityWithAutoKey.MaxAsync(m => (int?)m.Id);

        var entities = Enumerable.Range(1, count).Select(m => new EntityWithAutoKey
        {
            Id = maxId.Get(1) + m,
            Value = m,
            Name = Guid.NewGuid().ToString(),
        }).ToArray();

        var rows = await db.Database.GetDbConnection().BulkInsertAsync(entities, db.Schema, true);
        Assert.Equal(count, rows);

        foreach (var entity in entities)
        {
            var e = await db.EntityWithAutoKey.Where(m => m.Name == entity.Name).FirstAsync();
            Assert.Equal(e.Value, entity.Value);
            Assert.Equal(e.Id, entity.Id);
        }

        var ids = entities.Select(m => m.Id).ToArray();
        await db.EntityWithAutoKey.Where(m => ids.Contains(m.Id)).ExecuteDeleteAsync();
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task GetAsync_EntityWithGuidKey_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var entity = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1
        };
        await db.EntityWithGuidKey.AddAsync(entity);
        await db.SaveChangesAsync();

        var e = await db.Database.GetDbConnection().GetAsync<EntityWithGuidKey>(entity.Id, db.Schema);
        Assert.NotNull(e);
        Assert.Equal(entity.Value, e.Value);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task GetAsync_EntityWithoutKey_RaiseException(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var con = db.Database.GetDbConnection();
        var ex = await Assert.ThrowsAsync<DataException>(() => con.GetAsync<EntityWithoutKey>(0, "test"));
        Assert.Contains("Only supports an entity with a [Key] property", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DeleteAsync_EntityWithGuidKey_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var entities = Enumerable.Range(1, 3).Select(m => new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = m,
        }).ToArray();
        await db.EntityWithGuidKey.AddRangeAsync(entities);
        await db.SaveChangesAsync();

        var count = await db.Database.GetDbConnection().DeleteAsync<EntityWithGuidKey>(entities.First().Id, db.Schema);
        Assert.Equal(1, count);

        foreach (var e in entities.Skip(1))
        {
            await db.EntityWithGuidKey.AnyAsync(m => m.Id == e.Id);
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DeleteAsync_EntityWithoutKey_RaiseException(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var con = db.Database.GetDbConnection();
        var ex = await Assert.ThrowsAsync<DataException>(() => con.DeleteAsync<EntityWithoutKey>(0, "test"));
        Assert.Contains("Only supports an entity with a [Key] property", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DoTransactionAsync_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);

        var con = db.Database.GetDbConnection();

        var entity = new EntityWithAutoKey
        {
            Value = 100,
            Name = Guid.NewGuid().ToString(),
        };
        var entity2 = new EntityWithAutoKey
        {
            Value = 100,
            Name = Guid.NewGuid().ToString(),
        };

        var (id, id2) = await con.DoTransactionAsync(async tran =>
        {
            var id = (long?)await tran.InsertAsync(entity, db.Schema);
            var id2 = (long?)await tran.InsertAsync(entity2, db.Schema);
            return (id, id2);
        });

        var e = await db.EntityWithAutoKey.Where(m => m.Id == id).FirstOrDefaultAsync();
        Assert.NotNull(e);

        var e2 = await db.EntityWithAutoKey.Where(m => m.Id == id2).FirstOrDefaultAsync();
        Assert.NotNull(e2);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DoTransactionAsync_Rollback_Test(DbProviderType dbProviderType, string schema)
    {
        await using var db = Fixture.CreateDbContext(dbProviderType, schema);
        await db.EntityWithGuidKey.ExecuteDeleteAsync();

        var con = db.Database.GetDbConnection();

        await Assert.ThrowsAsync<InvalidOperationException>(() => con.DoTransactionAsync(async tran =>
        {
            await tran.InsertAsync(new EntityWithGuidKey
            {
                Id = Guid.NewGuid(),
                Value = 100,
            }, db.Schema);
            throw new InvalidOperationException();
        }));

        var count = await db.EntityWithGuidKey.CountAsync();
        Assert.Equal(0, count);
    }
}