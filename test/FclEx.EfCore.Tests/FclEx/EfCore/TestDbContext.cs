using MySql.Data.MySqlClient;
using System.Collections.Concurrent;
using Oracle.ManagedDataAccess.Client;

#if NET10_0_OR_GREATER
using Microting.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Microting.EntityFrameworkCore.MySql.Storage.Internal;
using Microting.EntityFrameworkCore.MySql.Infrastructure;
#else
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
#endif

#pragma warning disable EF1001
namespace FclEx.EfCore;

// EfCore is used for helping us to do tests
public class TestDbContext(
    DbDriver dbDriver,
    string connectionString,
    string? schema = null)
    : SchemaDbContext(schema)
{

    public DbDriver DbDriver { get; } = dbDriver;

    private string _connectionString = connectionString;
    public string ConnectionString => _connectionString;

    public DbSet<EntityWithAutoKey> EntityWithAutoKey { get; set; }
    public DbSet<EntityWithGuidKey> EntityWithGuidKey { get; set; }
    public DbSet<EntityWithoutKey> EntityWithoutKey { get; set; }

    public DbSet<HasPostfixEntity> HasPostfix { get; set; }
    public DbSet<HasTableAttributeEntity> HasTableAttribute { get; set; }
    public DbSet<EntityWithIdAndIndex> EntityWithIdAndIndex { get; set; }

    public DbSet<EntityHasStates> EntityHasStates { get; set; }
    public DbSet<EntityWithNavigation> EntityWithNavigation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);

        switch (DbDriver)
        {
            case DbDriver.SqlServer:
                builder.UseSqlServer(ConnectionString);
                break;
            case DbDriver.Sqlite:
                builder.UseSqlite(ConnectionString);
                break;
            case DbDriver.Npgsql:
                builder.UseNpgsql(ConnectionString);
                break;
            case DbDriver.MySql:
                UseMySQL(builder, ref _connectionString, Schema);
                break;
            case DbDriver.MySqlConnector:
                UseMySql(builder, ref _connectionString, Schema);
                break;
            case DbDriver.Oracle:
                UseOracle(builder, ref _connectionString, Schema);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(DbDriver), DbDriver, null);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        if (DbDriver == DbDriver.Sqlite)
        {
            var e = modelBuilder.Entity<EntityWithSqliteBlob>();
        }

        if (DbDriver == DbDriver.SqlServer)
        {
            var e = modelBuilder.Entity<EntityWithSqlServerXml>();
        }

        if (DbDriver == DbDriver.Npgsql)
        {
            var e = modelBuilder.Entity<EntityWithPostgresqlJsonb>();
        }

        if (DbDriver is DbDriver.MySqlConnector or DbDriver.MySql)
        {
            var e = modelBuilder.Entity<EntityWithMySqlBlob>();
        }

        modelBuilder.Entity<EntityWithoutKey>().HasNoKey();

        modelBuilder.Entity<EntityWithIdAndIndex>(e =>
        {
            e.HasIndex(m => m.Name).IsUnique();
            e.HasIndex(m => m.Value);
        });

        modelBuilder.Entity<EntityWithNavigation>()
            .HasOne(m => m.Navigation)
            .WithMany()
            .HasForeignKey(m => m.NavigationId);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ChangeTracker.ApplyEntityStateRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ChangeTracker.ApplyEntityStateRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private static readonly ConcurrentDictionary<string, ServerVersion> MySqlServerVersions = new();

    private static void UseMySql(DbContextOptionsBuilder builder, ref string connectionString, string? schema)
    {
        var sb = new MySqlConnectionStringBuilder(connectionString);
        if (schema.IsNotEmpty())
        {
            sb.Database = schema;
        }
        var str = sb.ConnectionString;
        var ver = MySqlServerVersions.GetOrAdd(str, m => ServerVersion.AutoDetect(m));
        builder.UseMySql(str, ver, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (_, table) => table));
        builder.ReplaceService<ISqlGenerationHelper, CustomMySqlSqlGenerationHelper>();
        connectionString = str;
    }

    public class CustomMySqlSqlGenerationHelper(
        RelationalSqlGenerationHelperDependencies dependencies,
        IMySqlOptions options)
        : MySqlSqlGenerationHelper(dependencies, options)
    {
        public override string GetSchemaName(string name, string schema) => schema;
    }

    private static void UseMySQL(DbContextOptionsBuilder builder, ref string connectionString, string? schema)
    {
        var sb = new MySqlConnectionStringBuilder(connectionString);
        if (schema.IsNotEmpty())
        {
            sb.Database = schema;
        }
        var str = sb.ConnectionString;
        builder.UseMySQL(str);
        connectionString = str;
    }

    private static void UseOracle(DbContextOptionsBuilder builder, ref string connectionString, string? schema)
    {
        var sb = new OracleConnectionStringBuilder(connectionString);
        if (schema.IsNotEmpty())
        {
            sb.UserID = schema;
        }
        // oracle requires the username to be double-quoted to preserve case sensitivity
        sb.UserID = sb.UserID.EnsureDoubleQuoted();
        var str = sb.ConnectionString;
        builder.UseOracle(str);
        connectionString = str;
    }
}