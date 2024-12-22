using System.Threading.Tasks;
using FclEx.Dapper;
using FclEx.Data;
using Npgsql;

namespace FclEx.Benchmarks;

[MinIterationCount(15)]
[MinInvokeCount(4)]
[MemoryDiagnoser, StopOnFirstError]
public class EntityInsertBenchmark
{
    [GlobalSetup]
    public static async Task InitializeAsync()
    {
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
    public async Task Dapper_Insert()
    {
        await using var con = new NpgsqlConnection(GlobalDbContext.LocalPostgresqlConnectionString);
        var count = await con.BulkInsertAsync(Entities);
        Check.EqualTo(count, Entities.Length);
    }
}