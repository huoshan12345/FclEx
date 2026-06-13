// ReSharper disable AccessToDisposedClosure
// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public partial class DbConnectionExtensionsTests(ITestOutputHelper output, DapperTestsFixture fixture) : DapperTests(fixture)
{
    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithAutoKey_Test(DbDriver dbDriver, string? schema)
    {
        var entity = new EntityWithAutoKey
        {
            Value = Random.Shared.Next(),
            Name = Guid.NewGuid().ToString(),
        };
        using var con = Fixture.CreateDbConnection(dbDriver, schema);
        var id = (long?)await con.InsertAsync(entity, schema);
        Assert.NotNull(id);

        var e = await con.GetAsync<EntityWithAutoKey>(id, schema);
        Assert.NotNull(e);
        Assert.Equal(entity.Name, e.Name);
        Assert.Equal(entity.Value, e.Value);
        Assert.Equal(id, e.Id);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithAutoKey_IncludeAutoKey_Test(DbDriver dbDriver, string? schema)
    {
        Assert.SkipIfInGithubAction(); 

        using var con = Fixture.CreateDbConnection(dbDriver, schema);
        var maxId = await GetMaxId<EntityWithAutoKey>(con, schema) + 1;

        var entity = new EntityWithAutoKey
        {
            Id = maxId + dbDriver.ToInt(),
            Value = 100,
            Name = Guid.NewGuid().ToString(),
        };

        await con.InsertAsync(entity, schema, includeAutoKey: true);

        // need to delete the inserted data with specified id to avoid affecting sequence of auto key generation in later tests
        await using var _ = AsyncDisposable.Create(async () =>
        {
            await con.DeleteAsync<EntityWithAutoKey>(entity.Id, schema);
            await FixAutoIncrement<EntityWithAutoKey>(con, dbDriver, schema);
        });

        var e = await con.GetAsync<EntityWithAutoKey>(entity.Id, schema);

        Assert.NotNull(e);
        Assert.Equal(entity.Value, e.Value);
        Assert.Equal(entity.Id, e.Id);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithGuidKey_Test(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var entity = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 100,
            Order = null,
        };

        var value = await con.InsertAsync(entity, schema);
        Assert.Null(value);

        var e = await con.GetAsync<EntityWithGuidKey>(entity.Id, schema);
        Assert.NotNull(e);
        Assert.Equal(entity.Value, e.Value);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task InsertAsync_EntityWithoutKey_Test(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var entity = new EntityWithoutKey
        {
            Name = Guid.NewGuid().ToString(),
            Value = 1
        };
        await con.InsertAsync(entity, schema);

        var tableName = DapperHelper.GetTableNameWithSchema(con, schema, typeof(EntityWithoutKey));
        var sql = $"select * from {tableName} where {DapperHelper.GetQuotedColumnName<EntityWithoutKey>(con, m => m.Name)} = @Name";
        var e = await con.QueryFirstAsync<EntityWithoutKey>(sql, new { entity.Name });
        Assert.Equal(entity.Value, e.Value);
    }

    [Theory]
    [MemberData(nameof(BulkInsertTestCases))]
    public async Task BulkInsertAsync_EntityWithAutoKey_Test(DbDriver dbDriver, string? schema, int count)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var entities = Enumerable.Range(1, count).Select(m => new EntityWithAutoKey
        {
            Value = m,
            Name = Guid.NewGuid().ToString(),
        }).ToArray();

        var rows = await con.BulkInsertAsync(entities, schema);
        Assert.Equal(count, rows);

        var tableName = DapperHelper.GetTableNameWithSchema(con, schema, typeof(EntityWithAutoKey));
        var sql = $"select * from {tableName} where {DapperHelper.GetQuotedColumnName<EntityWithAutoKey>(con, m => m.Name)} = @Name";
        foreach (var entity in entities)
        {
            var e = await con.QueryFirstAsync<EntityWithAutoKey>(sql, new { entity.Name });
            Assert.Equal(e.Value, entity.Value);
        }
    }

    [Theory]
    [MemberData(nameof(BulkInsertTestCases))]
    public async Task BulkInsertAsync_EntityWithAutoKey_IncludeAutoKey_Test(DbDriver dbDriver, string? schema, int count)
    {
        Assert.SkipIfInGithubAction();

        using var con = Fixture.CreateDbConnection(dbDriver, schema);
        var tableName = DapperHelper.GetTableNameWithSchema(con, schema, typeof(EntityWithAutoKey));
        var maxId = await GetMaxId<EntityWithAutoKey>(con, schema) + 1;

        var seed = (dbDriver.ToInt() + count) * 10;
        var entities = Enumerable.Range(1, count).Select(m => new EntityWithAutoKey
        {
            Id = maxId + m + seed,
            Value = m,
            Name = Guid.NewGuid().ToString(),
        }).ToArray();

        var rows = await con.BulkInsertAsync(entities, schema, true);
        Assert.Equal(count, rows);

        if (count == 0)
            return;

        // need to delete the inserted data with specified id to avoid affecting sequence of auto key generation in later tests
        await using var _ = AsyncDisposable.Create(async () =>
        {
            var parameters = entities.ToDynamicParameters((m, i) => i.ToString(), (m, i) => m.Id);
            var names = parameters.PrefixedNames().JoinWith(", ");
            var sql = $"delete from {tableName} where {DapperHelper.GetQuotedColumnName<EntityWithAutoKey>(con, m => m.Id)} in ({names})";
            await con.ExecuteAsync(sql, parameters);
            await FixAutoIncrement<EntityWithAutoKey>(con, dbDriver, schema);
        });

        var sql = $"select * from {tableName} where {DapperHelper.GetQuotedColumnName<EntityWithAutoKey>(con, m => m.Name)} = @Name";
        foreach (var entity in entities)
        {
            var e = await con.QueryFirstAsync<EntityWithAutoKey>(sql, new { entity.Name });
            Assert.Equal(e.Value, entity.Value);
            Assert.Equal(e.Id, entity.Id);
        }
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task GetAsync_EntityWithGuidKey_Test(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var entity = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1,
        };

        await con.InsertAsync(entity, schema);

        var e = await con.GetAsync<EntityWithGuidKey>(entity.Id, schema);
        Assert.NotNull(e);
        Assert.Equal(entity.Value, e.Value);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task GetAsync_EntityWithoutKey_RaiseException(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);
        var ex = await Assert.ThrowsAsync<DataException>(() => con.GetAsync<EntityWithoutKey>(0, schema));
        Assert.Contains("Only supports an entity with a [Key] property", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DeleteAsync_EntityWithGuidKey_Test(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var entities = Enumerable.Range(1, 3).Select(m => new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = m,
        }).ToArray();

        await con.BulkInsertAsync(entities, schema);

        var count = await con.DeleteAsync<EntityWithGuidKey>(entities.First().Id, schema);
        Assert.Equal(1, count);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DeleteAsync_EntityWithoutKey_RaiseException(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);
        var ex = await Assert.ThrowsAsync<DataException>(() => con.DeleteAsync<EntityWithoutKey>(0, schema));
        Assert.Contains("Only supports an entity with a [Key] property", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DoTransactionAsync_Test(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var entity = new EntityWithAutoKey
        {
            Value = 100,
            Name = Guid.NewGuid().ToString(),
        };
        var entity2 = new EntityWithAutoKey
        {
            Value = 200,
            Name = Guid.NewGuid().ToString(),
        };

        var (id, id2) = await con.DoTransactionAsync(async tran =>
        {
            var id = (long?)await tran.InsertAsync(entity, schema);
            var id2 = (long?)await tran.InsertAsync(entity2, schema);
            return (id1: id, id2);
        });

        Assert.NotNull(id);
        var e = await con.GetAsync<EntityWithAutoKey>(id, schema);
        Assert.NotNull(e);
        Assert.Equal(entity.Name, e.Name);

        Assert.NotNull(id2);
        var e2 = await con.GetAsync<EntityWithAutoKey>(id2, schema);
        Assert.NotNull(e2);
        Assert.Equal(entity2.Name, e2.Name);
    }

    [Theory]
    [MemberData(nameof(DbSchemaTestCases))]
    public async Task DoTransactionAsync_Rollback_Test(DbDriver dbDriver, string? schema)
    {
        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var count = await GetCount<EntityWithGuidKey>(con, schema);

        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(() => con.DoTransactionAsync(async tran =>
        {
            await tran.InsertAsync(new EntityWithGuidKey
            {
                Id = id,
                Value = 100,
            }, schema);
            throw new InvalidOperationException();
        }));

        var e = await con.GetAsync<EntityWithGuidKey>(id, schema);
        Assert.Null(e);
    }

    private static Task<int> GetCount<T>(IDbConnection con, string? schema)
    {
        var tableName = DapperHelper.GetTableNameWithSchema(con, schema, typeof(T));
        var sql = $"select count(1) from {tableName}";
        return con.ExecuteScalarAsync<int>(sql);
    }

    private static Task<int> GetMaxId<T>(IDbConnection con, string? schema)
    {
        var tableName = DapperHelper.GetTableNameWithSchema(con, schema, typeof(T));
        var columnName = DapperHelper.GetQuotedColumnName(con, typeof(T), "Id");
        return con.ExecuteScalarAsync<int>($"select max({columnName}) from {tableName}");
    }
}