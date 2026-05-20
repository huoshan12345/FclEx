using MySql.Data.MySqlClient;

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

    public DbDriver DbProviderType { get; } = dbDriver;
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

        switch (DbProviderType)
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
                UseMySQL(builder, ConnectionString, Schema);
                break;
            case DbDriver.MySqlConnector:
                UseMySql(builder, ConnectionString, Schema);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(DbProviderType), DbProviderType, null);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        if (DbProviderType == DbDriver.Sqlite)
        {
            var e = modelBuilder.Entity<EntityWithSqliteBlob>();
        }

        if (DbProviderType == DbDriver.SqlServer)
        {
            var e = modelBuilder.Entity<EntityWithSqlServerXml>();
        }

        if (DbProviderType == DbDriver.Npgsql)
        {
            var e = modelBuilder.Entity<EntityWithPostgresqlJsonb>();
        }

        if (DbProviderType is DbDriver.MySqlConnector or DbDriver.MySql)
        {
            var e = modelBuilder.Entity<EntityWithMySqlBlob>();
        }

        modelBuilder.Entity<EntityWithoutKey>().HasNoKey();

        modelBuilder.Entity<EntityWithIdAndIndex>().HasIndex(e => e.Name).IsUnique();
        modelBuilder.Entity<EntityWithIdAndIndex>().HasIndex(e => e.Value);

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

    private static void UseMySql(DbContextOptionsBuilder builder, string connectionString, string? schema)
    {
        var sb = new MySqlConnectionStringBuilder(connectionString);
        if (schema.IsNotEmpty())
        {
            sb.Database = schema;
        }
        var ver = ServerVersion.AutoDetect(connectionString);
        builder.UseMySql(sb.ConnectionString, ver, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (_, table) => table));
        builder.ReplaceService<ISqlGenerationHelper, CustomMySqlSqlGenerationHelper>();
    }

    public class CustomMySqlSqlGenerationHelper(
        RelationalSqlGenerationHelperDependencies dependencies,
        IMySqlOptions options)
        : MySqlSqlGenerationHelper(dependencies, options)
    {
        public override string GetSchemaName(string name, string schema) => schema;
    }

    private static void UseMySQL(DbContextOptionsBuilder builder, string connectionString, string? schema)
    {
        var sb = new MySqlConnectionStringBuilder(connectionString);
        if (schema.IsNotEmpty())
        {
            sb.Database = schema;
        }
        builder.UseMySQL(sb.ConnectionString);
    }
}