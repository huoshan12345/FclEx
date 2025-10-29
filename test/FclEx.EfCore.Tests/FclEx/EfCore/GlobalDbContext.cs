#pragma warning disable EF1001

namespace FclEx.EfCore;

public enum DbProviderType
{
    SqlServer,
    Sqlite,
#if !DISABLE_SOME_DATABASES
    Npgsql,
    MySql,
    MySqlConnector,
#endif
}

// EfCore is used for helping us to do tests
public class GlobalDbContext(
    DbProviderType dbProviderType,
    string connectionString,
    string? schema = null)
    : SchemaDbContext(schema)
{

    public DbProviderType DbProviderType { get; } = dbProviderType;
    public string ConnectionString { get; } = connectionString;

    public DbSet<EntityWithAutoKey> EntityWithAutoKey { get; set; }
    public DbSet<EntityWithGuidKey> EntityWithGuidKey { get; set; }
    public DbSet<EntityWithoutKey> EntityWithoutKey { get; set; }

    public DbSet<HasPostfixEntity> HasPostfix { get; set; }
    public DbSet<HasTableAttributeEntity> HasTableAttribute { get; set; }
    public DbSet<EntityWithIndex> EntityWithIdAndIndex { get; set; }

    public DbSet<EntityHasStates> EntityHasStates { get; set; }
    public DbSet<EntityWithNavigation> EntityWithNavigation { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);

        switch (DbProviderType)
        {
            case DbProviderType.SqlServer:
                builder.UseSqlServer(ConnectionString);
                break;
            case DbProviderType.Sqlite:
                builder.UseSqlite(ConnectionString);
                break;
#if !DISABLE_SOME_DATABASES
            case DbProviderType.Npgsql:
                builder.UseNpgsql(ConnectionString);
                break;
            case DbProviderType.MySql:
                builder.UseMySQL(ConnectionString);
                break;
            case DbProviderType.MySqlConnector:
                UseMySql(builder, ConnectionString, Schema);
                break;
#endif
            default:
                throw new ArgumentOutOfRangeException(nameof(DbProviderType), DbProviderType, null);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        if (DbProviderType == DbProviderType.Sqlite)
        {
            var e = modelBuilder.Entity<EntityWithSqliteBlob>();
        }

        if (DbProviderType == DbProviderType.SqlServer)
        {
            var e = modelBuilder.Entity<EntityWithSqlServerXml>();
        }

#if !DISABLE_SOME_DATABASES
        if (DbProviderType == DbProviderType.Npgsql)
        {
            var e = modelBuilder.Entity<EntityWithPostgresqlJsonb>();
        }

        if (DbProviderType is DbProviderType.MySqlConnector or DbProviderType.MySql)
        {
            var e = modelBuilder.Entity<EntityWithMySqlBlob>();
        }
#endif
        modelBuilder.Entity<EntityWithoutKey>().HasNoKey();

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

#if !DISABLE_SOME_DATABASES
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
#endif
}