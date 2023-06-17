using System;
using System.Linq;
using System.Threading.Tasks;
using FclEx.Abp.OrmLite;
using Npgsql;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.PostgreSQL;
using ServiceStack.Text;
#pragma warning disable CA1822

namespace FclEx.Abp.Benchmarks.Data;

[MinIterationCount(15)]
[MinInvokeCount(4)]
[MemoryDiagnoser, StopOnFirstError]
public class EntityInsertBenchmark
{
    private static IOrmLiteDialectProvider PostgreSqlDialectProvider { get; }
        = new PostgreSqlDialectProvider { NamingStrategy = new OrmLiteNamingStrategyBase() };
    private static OrmLiteConnectionFactory ConnectionFactory { get; } = new(GlobalDbContext.LocalPostgresqlConnectionString, PostgreSqlDialectProvider);

    [GlobalSetup]
    public static async Task InitializeAsync()
    {
        JsConfig.Reset(); // To initialize ServiceStack cache, prevent it initializing at an unexpected time.
        OrmLiteConfig.StripUpperInLike = true; // NOTE, if it is false, query contains "like" will be very slow.
        AttributeHelper.AddOrmLiteAttribute(typeof(EntityWithAutoKey));

        await using var context = new GlobalDbContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public static readonly EntityWithAutoKey[] Entities = Enumerable.Range(1, 10000).Select(m => new EntityWithAutoKey
    {
        Name = Guid.NewGuid().ToString(),
        Value = m,
    }).ToArray();

    [Benchmark]
    public async Task EfCore_Insert()
    {
        await using var context = new GlobalDbContext();
        context.EntityWithAutoKeys.AddRange(Entities);
        var count = await context.SaveChangesAsync();
        Check.EqualTo(count, Entities.Length);
    }

    [Benchmark]
    public async Task OrmLite_Insert()
    {
        using var con = await ConnectionFactory.OpenAsync();
        var count = await con.InsertBulkAsync(Entities);
        Check.EqualTo(count, Entities.Length);
    }
}