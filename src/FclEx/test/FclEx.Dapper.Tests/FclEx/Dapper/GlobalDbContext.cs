using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
#pragma warning disable EF1001

namespace FclEx.Dapper;

public enum DatabaseType
{
    Npgsql,
    SqlServer,
    Sqlite,
    MySql,
    MySqlConnector,
}

// EfCore is used for helping us to do tests
public class GlobalDbContext : DbContextWithSchema
{
    public static readonly string DatabaseName = typeof(GlobalDbContext).Assembly.GetName().Name!.Replace(".", "-").ToLower();
    public static readonly string PostgresqlConnectionString = $"Server=localhost;Database={DatabaseName};Port=5432;User Id=postgres;Password=111111";
    public static readonly string SqlServerConnectionString = $@"Data Source=localhost\sqlexpress;Database={DatabaseName};User Id=sa;Password=a.o7a@bj;Integrated Security=sspi;Encrypt=false";
    public static readonly string MySqlConnectionString = $@"Server=localhost;Database={DatabaseName};Port=3306;User Id=root;Password=111111;SslMode=Required";
    public static readonly string SqliteConnectionString = $@"Data Source=./{DatabaseName}.sqlite;";

    public DatabaseType DatabaseType { get; }
    private readonly Action<DbContextOptionsBuilder> _optionsAction;

    public GlobalDbContext(DatabaseType databaseType, Action<DbContextOptionsBuilder> optionsAction, string? schema) : base(schema)
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
        }

        if (DatabaseType == DatabaseType.SqlServer)
        {
            var e = modelBuilder.Entity<EntityWithSqlServerXml>();
        }

        if (DatabaseType == DatabaseType.Sqlite)
        {
            var e = modelBuilder.Entity<EntityWithSqliteBlob>();
        }

        if (DatabaseType is DatabaseType.MySqlConnector or DatabaseType.MySql)
        {
            var e = modelBuilder.Entity<EntityWithMySqlBlob>();
        }

        modelBuilder.Entity<EntityWithoutKey>().HasNoKey();

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entity.Name)
                .ToTable(entity.ClrType.Name);
        }
    }

    private static void UseMySql(DbContextOptionsBuilder builder, string connectionString)
    {
        var ver = ServerVersion.AutoDetect(connectionString);
        builder.UseMySql(connectionString, ver, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => table));
        builder.ReplaceService<ISqlGenerationHelper, CustomMySqlSqlGenerationHelper>();
    }

    public static GlobalDbContext Create(DatabaseType databaseType, string? schema = null)
    {
        return databaseType switch
        {
            DatabaseType.Npgsql => new(databaseType, builder => builder.UseNpgsql(PostgresqlConnectionString), schema),
            DatabaseType.SqlServer => new(databaseType, builder => builder.UseSqlServer(SqlServerConnectionString), schema),
            DatabaseType.MySql => new(databaseType, builder => builder.UseMySQL(MySqlConnectionString), null),
            DatabaseType.MySqlConnector => new(databaseType, builder => UseMySql(builder, MySqlConnectionString), schema),
            DatabaseType.Sqlite => new(databaseType, builder => builder.UseSqlite(SqliteConnectionString), null),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType, null),
        };
    }
}

public class CustomMySqlSqlGenerationHelper : MySqlSqlGenerationHelper
{
    public CustomMySqlSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies, IMySqlOptions options)
        : base(dependencies, options)
    {
    }

    public override string GetSchemaName(string name, string schema) => schema;
}