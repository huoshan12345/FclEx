namespace FclEx.EfCore;

/// <summary>A minimal model used only for isolated truncate tests.</summary>
internal sealed class TruncateDbContext(
    DbDriver driver, string connectionString, string? schema = null, DbContextOptions<TestDbContext>? options = null)
    : TestDbContext(driver, connectionString, schema, options)
{
}
