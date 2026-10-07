using System.Collections.Concurrent;

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

    public string ConnectionString { get; } = connectionString;

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
                builder.UseMySQL(ConnectionString);
                break;
            case DbDriver.MySqlConnector:
                UseMySql(builder, ConnectionString);
                break;
            case DbDriver.Oracle:
                builder.UseOracle(ConnectionString);
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

    private static void UseMySql(DbContextOptionsBuilder builder, string connectionString)
    {
        var ver = MySqlServerVersions.GetOrAdd(connectionString, m => ServerVersion.AutoDetect(m));
        builder.UseMySql(connectionString, ver, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (_, table) => table));
        builder.ReplaceService<ISqlGenerationHelper, CustomMySqlSqlGenerationHelper>();
    }

    public class CustomMySqlSqlGenerationHelper(
        RelationalSqlGenerationHelperDependencies dependencies,
        IMySqlOptions options)
        : MySqlSqlGenerationHelper(dependencies, options)
    {
        public override string GetSchemaName(string name, string schema) => schema;
    }

}
