namespace FclEx.EfCore;

public class DataAnnotationTests(EfCoreFixture fixture) : EfCoreTests(fixture)
{
#if !DISABLE_NPGSQL
    [Fact]
    public async Task TableName_Postfix_Test()
    {
        await using var context = Fixture.CreateDbContext(DbProviderType.Npgsql);
        var e = new HasPostfixEntity();
        await context.InsertAsync(e);

        var con = context.Database.GetDbConnection();
        // use double-quoted to prevent PostgreSQL from folding into lowercase
        var count = await con.ExecuteScalarAsync<int>("select count(1) from \"HasPostfix\" where \"Id\" = @Id", new { e.Id });
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task TableName_TableAttribute_Test()
    {
        await using var context = Fixture.CreateDbContext(DbProviderType.Npgsql);
        var e = new HasTableAttributeEntity();
        await context.InsertAsync(e);

        var con = context.Database.GetDbConnection();
        // use double-quoted to prevent PostgreSQL from folding into lowercase
        var count = await con.ExecuteScalarAsync<int>("select count(1) from \"has_table_name\" where \"Id\" = @Id", new { e.Id });
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task IEntity_Id_AutoIncrement_Test()
    {
        await using var context = Fixture.CreateDbContext(DbProviderType.Npgsql);
        var e = new EntityWithIndex();
        await context.InsertAsync(e);
        Assert.NotEqual(0, e.Id);
    }

    [Fact]
    public async Task IEntity_Id_PrimaryKey_Test()
    {
        await using var context = Fixture.CreateDbContext(DbProviderType.Npgsql);
        var entityType = context.Model.FindEntityType(typeof(EntityWithIndex));
        Assert.NotNull(entityType);
        var (table, schema) = (entityType.GetTableName(), entityType.GetSchema() ?? "public");

        const string sql = """
            select c.column_name from information_schema.columns c 
            JOIN information_schema.constraint_column_usage ccu on ccu.table_schema = c.table_schema and ccu.table_name = c.table_name and ccu.column_name = c.column_name
            JOIN information_schema.table_constraints tc on tc.table_schema = c.table_schema and tc.table_name = c.table_name
            where 1 = 1
            and tc.constraint_type = 'PRIMARY KEY'
            and c.table_name = @Table
            and c.table_schema = @Schema
            """;

        var con = context.Database.GetDbConnection();
        var enumerable = await con.QueryAsync<string>(sql, new { Table = table, Schema = schema });
        var list = enumerable.AsIList();

        Assert.Single(list);
        Assert.Equal(nameof(EntityWithIndex.Id), list[0]);
    }

    [Fact]
    public async Task IEntity_Indexes_Test()
    {
        await using var context = Fixture.CreateDbContext(DbProviderType.Npgsql);
        var entityType = context.Model.FindEntityType(typeof(EntityWithIndex));
        Assert.NotNull(entityType);
        var (table, schema) = (entityType.GetTableName(), entityType.GetSchema() ?? "public");

        const string sql = """
            select a.attname, idx.indisunique 
            from pg_indexes i
            join pg_class c on c.relname = i.indexname
            join pg_attribute a on a.attrelid = c.oid
            join pg_index idx on idx.indexrelid = c.oid
            JOIN pg_namespace AS ns ON c.relnamespace = ns.oid and ns.nspname = schemaname
            where 1 = 1
            and tablename = @Table
            and schemaname = @Schema
            """;

        var con = context.Database.GetDbConnection();
        var enumerable = await con.QueryAsync<(string, bool)>(sql, new { Table = table, Schema = schema });
        var list = enumerable.OrderBy(m => m.Item1).AsIList();

        Assert.Equal(3, list.Count);
        Assert.Equal((nameof(EntityWithIndex.Id), true), list[0]);
        Assert.Equal((nameof(EntityWithIndex.Name), true), list[1]);
        Assert.Equal((nameof(EntityWithIndex.Value), false), list[2]);
    }
#endif
}
