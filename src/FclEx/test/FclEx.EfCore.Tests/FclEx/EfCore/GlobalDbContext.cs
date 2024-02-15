using Microsoft.EntityFrameworkCore.Storage;
using MySql.Data.MySqlClient;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;

#pragma warning disable EF1001

namespace FclEx.EfCore;

public enum DbProviderType
{
    Npgsql,
    SqlServer,
    Sqlite,
    MySql,
    MySqlConnector,
}

// EfCore is used for helping us to do tests
public class GlobalDbContext : SchemaDbContext
{
    public DbProviderType DbProviderType { get; }
    public string ConnectionString { get; }
    private readonly Action<DbContextOptionsBuilder>? _optionsAction;

    public GlobalDbContext(DbProviderType dbProviderType, string connectionString, Action<DbContextOptionsBuilder>? optionsAction = null, string? schema = null)
        : base(schema)
    {
        DbProviderType = dbProviderType;
        ConnectionString = connectionString;
        _optionsAction = optionsAction;
    }


    public GlobalDbContext(DbProviderType dbProviderType, bool isUser, Action<DbContextOptionsBuilder>? optionsAction = null, string? schema = null)
        : base(schema)
    {
        DbProviderType = dbProviderType;
        ConnectionString = ConnectionStrings.Get(dbProviderType, isUser);
        _optionsAction = optionsAction;
    }

    public DbSet<EntityWithAutoKey> EntityWithAutoKeys { get; set; } = default!;
    public DbSet<EntityWithGuidKey> EntityWithGuidKeys { get; set; } = default!;
    public DbSet<EntityWithoutKey> EntityWithoutKeys { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);

        switch (DbProviderType)
        {
            case DbProviderType.Npgsql:
                builder.UseNpgsql(ConnectionString);
                break;
            case DbProviderType.SqlServer:
                builder.UseSqlServer(ConnectionString);
                break;
            case DbProviderType.MySql:
                builder.UseMySQL(ConnectionString);
                break;
            case DbProviderType.MySqlConnector:
                UseMySql(builder, ConnectionString, Schema);
                break;
            case DbProviderType.Sqlite:
                builder.UseSqlite(ConnectionString);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(DbProviderType), DbProviderType, null);
        }

        _optionsAction?.Invoke(builder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (DbProviderType == DbProviderType.Npgsql)
        {
            var e = modelBuilder.Entity<EntityWithPostgresqlJsonb>();
        }

        if (DbProviderType == DbProviderType.SqlServer)
        {
            var e = modelBuilder.Entity<EntityWithSqlServerXml>();
        }

        if (DbProviderType == DbProviderType.Sqlite)
        {
            var e = modelBuilder.Entity<EntityWithSqliteBlob>();
        }

        if (DbProviderType is DbProviderType.MySqlConnector or DbProviderType.MySql)
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

    private static void UseMySql(DbContextOptionsBuilder builder, string connectionString, string? schema)
    {
        var sb = new MySqlConnectionStringBuilder(connectionString);
        if (schema.IsNotEmpty())
        {
            sb.Database = schema;
        }
        var ver = ServerVersion.AutoDetect(connectionString);
        builder.UseMySql(sb.ConnectionString, ver, o => o.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => table));
        builder.ReplaceService<ISqlGenerationHelper, CustomMySqlSqlGenerationHelper>();
    }

    public static GlobalDbContext Create(DbProviderType dbProviderType, string? schema = null, bool isUser = false)
    {
        return new(dbProviderType, isUser, null, schema);
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