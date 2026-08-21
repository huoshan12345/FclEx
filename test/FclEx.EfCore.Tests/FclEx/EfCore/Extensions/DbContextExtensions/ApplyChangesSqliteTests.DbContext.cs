// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace FclEx.EfCore.Extensions.DbContextExtensions;

partial class ApplyChangesSqliteTests
{
    private sealed record ItemDto(string Code, string Name, string ProtectedValue = "");

    private sealed record ChildDto(string Code, string Name);

    private sealed record ManualKeyDto(string Id, string Name);

    private sealed class Item : ISoftDeletable
    {
        public long Id { get; set; }

        public string Code { get; set; } = "";

        public string Name { get; set; } = "";

        public string ProtectedValue { get; set; } = "";

        public bool IsDeleted { get; set; }
    }

    private sealed class Parent
    {
        public long Id { get; set; }

        public string Name { get; set; } = "";

        public List<Child> Children { get; set; } = [];
    }

    private sealed class Child
    {
        public long Id { get; set; }

        public long ParentId { get; set; }

        public Parent Parent { get; set; } = null!;

        public string Code { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private sealed class ManualKeyItem
    {
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private sealed class ShadowKeyItem
    {
        public string Code { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private sealed class ApplyChangesDbContext(DbContextOptions<ApplyChangesDbContext> options) : DbContext(options)
    {
        public DbSet<Item> Items => Set<Item>();

        public DbSet<Parent> Parents => Set<Parent>();

        public DbSet<Child> Children => Set<Child>();

        public DbSet<ManualKeyItem> ManualKeyItems => Set<ManualKeyItem>();

        public DbSet<ShadowKeyItem> ShadowKeyItems => Set<ShadowKeyItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                builder.Property(entity => entity.Code).IsRequired();
                builder.Property(entity => entity.Name).IsRequired();
                builder.Property(entity => entity.ProtectedValue).IsRequired();
            });

            modelBuilder.Entity<Parent>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                builder.Property(entity => entity.Name).IsRequired();
            });

            modelBuilder.Entity<Child>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
                builder.Property(entity => entity.Code).IsRequired();
                builder.Property(entity => entity.Name).IsRequired();
                builder.HasOne(entity => entity.Parent)
                    .WithMany(entity => entity.Children)
                    .HasForeignKey(entity => entity.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ManualKeyItem>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Id).ValueGeneratedNever();
                builder.Property(entity => entity.Name).IsRequired();
            });

            modelBuilder.Entity<ShadowKeyItem>(builder =>
            {
                builder.Property<long>("Id").ValueGeneratedOnAdd();
                builder.HasKey("Id");
                builder.Property(entity => entity.Code).IsRequired();
                builder.Property(entity => entity.Name).IsRequired();
            });
        }
    }

    private sealed class SqliteTestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplyChangesDbContext> _options;

        private SqliteTestDatabase(SqliteConnection connection)
        {
            _connection = connection;
            _options = new DbContextOptionsBuilder<ApplyChangesDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .LogTo(m => TestContext.Current.TestOutputHelper?.WriteLine(m), LogLevel.Information)
                .Options;
        }

        public static async Task<SqliteTestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SqliteTestDatabase(connection);
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();
            return database;
        }

        public ApplyChangesDbContext CreateContext() => new(_options);

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }
}
