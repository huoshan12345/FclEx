namespace FclEx.Abp.EfCore;

public class GlobalDbContext : DbContext
{
    public static readonly string DatabaseName = typeof(GlobalDbContext).Assembly.GetName().Name!.Replace(".", "-").ToLower();
    public static readonly string LocalPostgresqlConnectionString = $"Server=localhost;Database={DatabaseName};Port=5432;User Id=postgres;Password=111111";

    public string ConnectionString { get; }

    public GlobalDbContext(string? connectionString = null)
    {
        ConnectionString = connectionString ?? LocalPostgresqlConnectionString;
    }

    public DbSet<HasPostfixEntity> HasPostfix { get; set; } = default!;
    public DbSet<HasTableAttributeEntity> HasTableAttribute { get; set; } = default!;
    public DbSet<EntityWithIdAndIndex> EntityWithIdAndIndex { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.SetFclExAbpAttributes();
    }
}