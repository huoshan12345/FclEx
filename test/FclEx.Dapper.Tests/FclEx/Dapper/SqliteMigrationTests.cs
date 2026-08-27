// ReSharper disable UseAwaitUsing
namespace FclEx.Dapper;

public class SqliteMigrationTests
{
    [Fact]
    public async Task MigrateUp_CreatesInMemoryTableForDapperOperations()
    {
        using var database = await SqliteMigrationTestDatabase.CreateAsync();
        using var connection = database.CreateConnection();

        var entity = new SqliteMigrationTestEntity
        {
            Name = "created on demand",
            Value = 42,
        };

        var id = await connection.InsertAsync<SqliteMigrationTestEntity, long>(entity);

        var persisted = await connection.GetAsync<SqliteMigrationTestEntity>(id);
        Assert.NotNull(persisted);
        Assert.Equal(entity.Name, persisted.Name);
        Assert.Equal(entity.Value, persisted.Value);
    }

    [Fact]
    public async Task MigrateDown_RemovesInMemoryTable()
    {
        using var database = await SqliteMigrationTestDatabase.CreateAsync();

        database.MigrateDown();

        using var connection = database.CreateConnection();
        var tableCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @Name",
            new { Name = CreateSqliteMigrationTestEntities.TableName });

        Assert.Equal(0, tableCount);
    }
}

internal sealed class SqliteMigrationTestDatabase : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    private readonly ServiceProvider _serviceProvider;

    private SqliteMigrationTestDatabase(
        string connectionString,
        SqliteConnection keepAliveConnection,
        ServiceProvider serviceProvider)
    {
        ConnectionString = connectionString;
        _keepAliveConnection = keepAliveConnection;
        _serviceProvider = serviceProvider;
    }

    private string ConnectionString { get; }

    public static async Task<SqliteMigrationTestDatabase> CreateAsync()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = $"FclEx.Dapper.Tests.{Guid.NewGuid():N}",
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        }.ToString();

        var keepAliveConnection = new SqliteConnection(connectionString);
        await keepAliveConnection.OpenAsync();

        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateSqliteMigrationTestEntities).Assembly).For.Migrations())
            .BuildServiceProvider();

        var database = new SqliteMigrationTestDatabase(connectionString, keepAliveConnection, serviceProvider);
        database.MigrateUp();
        return database;
    }

    public SqliteConnection CreateConnection() => new(ConnectionString);

    public void MigrateDown() => Run(runner => runner.MigrateDown(0));

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _keepAliveConnection.Dispose();
    }

    private void MigrateUp() => Run(runner => runner.MigrateUp());

    private void Run(Action<IMigrationRunner> action)
    {
        using var scope = _serviceProvider.CreateScope();
        action(scope.ServiceProvider.GetRequiredService<IMigrationRunner>());
    }
}

[Migration(2026082201)]
public sealed class CreateSqliteMigrationTestEntities : Migration
{
    public const string TableName = nameof(SqliteMigrationTestEntity);

    public override void Up()
    {
        Create.Table(TableName)
            .WithColumn(nameof(SqliteMigrationTestEntity.Id)).AsInt64().PrimaryKey().Identity()
            .WithColumn(nameof(SqliteMigrationTestEntity.Name)).AsString(200).NotNullable()
            .WithColumn(nameof(SqliteMigrationTestEntity.Value)).AsInt32().NotNullable();
    }

    public override void Down()
    {
        Delete.Table(TableName);
    }
}

public sealed class SqliteMigrationTestEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    public string Name { get; set; } = "";

    public int Value { get; set; }
}
