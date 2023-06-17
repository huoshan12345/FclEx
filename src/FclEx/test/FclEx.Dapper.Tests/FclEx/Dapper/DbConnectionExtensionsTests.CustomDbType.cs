using Newtonsoft.Json;

namespace Seismic.Eds.Common.Dapper.Tests;

partial class DbConnectionExtensionsTests
{
    [Theory]
    [MemberData(nameof(SchemaCases))]
    public async Task InsertAsync_EntityWithPostgresqlJsonb_Test(string schema)
    {
        if (DatabaseTypes.Contains(DatabaseType.Npgsql) == false)
            return;

        await using var db = GlobalDbContext.Create(DatabaseType.Npgsql, schema);

        var payload = new EntityWithGuidKey
        {
            Id = Guid.NewGuid(),
            Value = 1
        };
        var entity = new EntityWithPostgresqlJsonb
        {
            Json = JsonConvert.SerializeObject(payload)
        };

        var id = (long?)await db.Database.GetDbConnection().InsertAsync(schema, entity);
        var e = await db.Set<EntityWithPostgresqlJsonb>().Where(m => m.Id == id).FirstOrDefaultAsync();
        Assert.NotNull(e);
        AssertExt.NotEmpty(e.Json);

        var actualPayload = JsonConvert.DeserializeObject<EntityWithGuidKey>(e.Json)!;
        Assert.Equal(payload.Id, actualPayload.Id);
        Assert.Equal(payload.Value, actualPayload.Value);
    }
}