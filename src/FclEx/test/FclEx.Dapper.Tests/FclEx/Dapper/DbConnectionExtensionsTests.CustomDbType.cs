namespace FclEx.Dapper;

partial class DbConnectionExtensionsTests
{
    [Theory]
    [MemberData(nameof(SchemaCases))]
    public async Task InsertAsync_EntityWithPostgresqlJsonb_Test(string schema)
    {
        using var x = output.SetConsole();

        await using var db = Fixture.CreateDbContext(DbProviderType.Npgsql, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1
        };
        var entity = new EntityWithPostgresqlJsonb
        {
            Json = JsonConvert.SerializeObject(payload)
        };

        var id = (long?)await db.Database.GetDbConnection().InsertAsync(entity, db.Schema);
        var e = await db.Set<EntityWithPostgresqlJsonb>().Where(m => m.Id == id).FirstOrDefaultAsync();
        Assert.NotNull(e);
        AssertExt.NotEmpty(e.Json);

        var actualPayload = JsonConvert.DeserializeObject<EntityWithGuidKey>(e.Json)!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }

    [LocalOnlyTheory]
    [MemberData(nameof(SchemaCases))]
    public async Task InsertAsync_EntityWithSqlServerXml_Test(string schema)
    {
        using var x = output.SetConsole();

        await using var db = Fixture.CreateDbContext(DbProviderType.SqlServer, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1
        };
        var entity = new EntityWithSqlServerXml
        {
            Xml = payload.ToXml()
        };

        var id = (long?)await db.Database.GetDbConnection().InsertAsync(entity, db.Schema);
        var e = await db.Set<EntityWithSqlServerXml>().Where(m => m.Id == id).FirstOrDefaultAsync();
        Assert.NotNull(e);
        AssertExt.NotEmpty(e.Xml);

        var actualPayload = XElement.Parse(e.Xml).ToObject<EntityWithGuidKey>();
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }

    [Fact]
    public async Task InsertAsync_EntityWithSqliteBlob_Test()
    {
        using var x = output.SetConsole();

        await using var db = Fixture.CreateDbContext(DbProviderType.Sqlite);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1
        };
        var entity = new EntityWithSqliteBlob
        {
            Blob = payload.ToJson().ToBytes(),
        };

        var id = (long?)await db.Database.GetDbConnection().InsertAsync(entity);
        var e = await db.Set<EntityWithSqliteBlob>().Where(m => m.Id == id).FirstOrDefaultAsync();
        Assert.NotNull(e);
        AssertExt.NotEmpty(e.Blob);

        var actualPayload = e.Blob.GetString().FromJson<EntityWithGuidKey>()!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }

    public static readonly IEnumerable<object?[]> MySqlSchemaCases = new[] { DbProviderType.MySqlConnector, DbProviderType.MySql, }
        .SelectMany(Schemas)
        .Select(m => new object?[] { m.Left, m.Right });

    [LocalOnlyTheory]
    [MemberData(nameof(MySqlSchemaCases))]
    public async Task InsertAsync_EntityWithMySqlBlob_Test(DbProviderType type, string schema)
    {
        using var x = output.SetConsole();

        await using var db = Fixture.CreateDbContext(type, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1
        };
        var entity = new EntityWithMySqlBlob
        {
            Blob = payload.ToJson().ToBytes(),
        };

        var id = (long?)await db.Database.GetDbConnection().InsertAsync(entity, db.Schema);
        var e = await db.Set<EntityWithMySqlBlob>().Where(m => m.Id == id).FirstOrDefaultAsync();
        Assert.NotNull(e);
        AssertExt.NotEmpty(e.Blob);

        var actualPayload = e.Blob.GetString().FromJson<EntityWithGuidKey>()!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }
}