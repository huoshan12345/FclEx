// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

partial class DbConnectionExtensionsTests
{
    [Theory]
    [MemberData(nameof(SchemaCases))]
    public async Task InsertAsync_EntityWithPostgresqlJsonb_Test(string? schema)
    {
        if (DbDrivers.Contains(DbDriver.Npgsql) == false)
            return;

        using var con = Fixture.CreateDbConnection(DbDriver.Npgsql, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1,
        };
        var entity = new EntityWithPostgresqlJsonb
        {
            Json = payload.ToJson(),
        };

        var id = (long?)await con.InsertAsync(entity, schema);
        Assert.NotNull(id);

        var e = await con.GetAsync<EntityWithPostgresqlJsonb>(id, schema);
        Assert.NotNull(e);
        Assert.NotNullNorEmpty(e.Json);

        var actualPayload = e.Json.FromJson<EntityWithGuidKey>()!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }

    [Theory]
    [MemberData(nameof(SchemaCases))]
    public async Task InsertAsync_EntityWithSqlServerXml_Test(string? schema)
    {
        if (DbDrivers.Contains(DbDriver.SqlServer) == false)
            return;

        using var con = Fixture.CreateDbConnection(DbDriver.SqlServer, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1,
        };
        var entity = new EntityWithSqlServerXml
        {
            Xml = XmlHelper.Serialize(payload),
        };

        var id = (long?)await con.InsertAsync(entity, schema);
        Assert.NotNull(id);

        var e = await con.GetAsync<EntityWithSqlServerXml>(id, schema);
        Assert.NotNull(e);
        Assert.NotNullNorEmpty(e.Xml);

        var actualPayload = XElement.Parse(e.Xml).ToObject<EntityWithGuidKey>();
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }

    [LocalOnlyFact]
    public async Task InsertAsync_EntityWithSqliteBlob_Test()
    {
        if (DbDrivers.Contains(DbDriver.Sqlite) == false)
            return;
        
        using var con = Fixture.CreateDbConnection(DbDriver.Sqlite, null);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1,
        };
        var entity = new EntityWithSqliteBlob
        {
            Blob = payload.ToJson().ToBytes(),
        };

        var id = (long?)await con.InsertAsync(entity);
        Assert.NotNull(id);

        var e = await con.GetAsync<EntityWithSqliteBlob>(id);
        Assert.NotNull(e);
        Assert.NotNullNorEmpty(e.Blob);

        var actualPayload = e.Blob.GetString().FromJson<EntityWithGuidKey>()!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }

    [LocalOnlyTheory]
    [MemberData(nameof(MySqlSchemaCases))]
    public async Task InsertAsync_EntityWithMySqlBlob_Test(DbDriver dbDriver, string? schema)
    {
        if (DbDrivers.Contains(DbDriver.MySql) == false)
            return;

        using var con = Fixture.CreateDbConnection(dbDriver, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1,
        };
        var entity = new EntityWithMySqlBlob
        {
            Blob = payload.ToJson().ToBytes(),
        };

        var id = (long?)await con.InsertAsync(entity, schema);
        Assert.NotNull(id);

        var e = await con.GetAsync<EntityWithMySqlBlob>(id, schema);
        Assert.NotNull(e);
        Assert.NotNullNorEmpty(e.Blob);

        var actualPayload = e.Blob.GetString().FromJson<EntityWithGuidKey>()!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }
}