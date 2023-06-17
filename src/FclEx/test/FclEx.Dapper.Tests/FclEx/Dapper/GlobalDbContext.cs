namespace FclEx.Dapper;

public enum DatabaseType
{
    Npgsql,
    SqlServer
}

// EfCore is used for helping us to do tests
public class GlobalDbContext : DbContextWithSchema
{
    public const string LocalPostgresqlConnectionString = "Server=localhost;Database=test;Port=5432;User Id=postgres;Password=111111";
    public const string LocalSqlServerConnectionString = @"Data Source=localhost\sqlexpress;Database=test;User Id=sa;Password=a.o7a@bj;Integrated Security=sspi;Encrypt=false";

    public DatabaseType DatabaseType { get; }
    private readonly Action<DbContextOptionsBuilder> _optionsAction;

    public GlobalDbContext(DatabaseType databaseType, Action<DbContextOptionsBuilder> optionsAction, string schema) : base(schema)
    {
        _optionsAction = optionsAction;
        DatabaseType = databaseType;
    }

    public DbSet<EntityWithAutoKey> EntityWithAutoKeys { get; set; } = default!;
    public DbSet<EntityWithGuidKey> EntityWithGuidKeys { get; set; } = default!;
    public DbSet<EntityWithoutKey> EntityWithoutKeys { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        _optionsAction(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (DatabaseType == DatabaseType.Npgsql)
        {
            var e = modelBuilder.Entity<EntityWithPostgresqlJsonb>();
            e.Property(m => m.Json).HasColumnType("jsonb");
        }
    }

    public static GlobalDbContext Create(DatabaseType databaseType, string schema)
    {
        return databaseType switch
        {
            DatabaseType.Npgsql => new(databaseType, builder => builder.UseNpgsql(LocalPostgresqlConnectionString), schema),
            DatabaseType.SqlServer => new(databaseType, builder => builder.UseSqlServer(LocalSqlServerConnectionString), schema),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null)
        };
    }
}