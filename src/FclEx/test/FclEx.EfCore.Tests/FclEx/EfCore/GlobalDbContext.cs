namespace FclEx.EfCore;

// EfCore is used for helping us to do tests
public class GlobalDbContext : DbContextWithSchema
{
    public string ConnectionString { get; }

    public GlobalDbContext(string connectionString, string? schema) : base(schema)
    {
        ConnectionString = connectionString;
    }

    public DbSet<EntityWithAutoKey> EntityWithAutoKeys { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}