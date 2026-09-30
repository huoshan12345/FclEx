using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FclEx.EfCore.Extensions;

internal sealed class TruncateTestContext(DbContextOptions options, string shape = "simple") : DbContext(options)
{
    internal string Shape => shape;

    internal static TruncateTestContext Create(int provider, string shape, CommandRecorder recorder)
    {
        var builder = new DbContextOptionsBuilder()
            .AddInterceptors(recorder)
            .ReplaceService<IModelCacheKeyFactory, ShapeModelCacheKeyFactory>();
        switch (provider)
        {
            case 0:
                builder.UseSqlServer("Server=localhost;Database=unused;Integrated Security=true;TrustServerCertificate=true");
                break;
            case 1:
                builder.UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused");
                break;
            case 2:
                builder.UseMySQL("Server=localhost;Database=unused;User=unused;Password=unused");
                break;
            case 3:
                builder.UseMySql("Server=localhost;Database=unused;User=unused;Password=unused",
                    new MySqlServerVersion(new Version(8, 0, 36)));
                break;
            default:
                builder.UseSqlite("Data Source=:memory:");
                if (provider == 5)
                    builder.ReplaceService<IDatabaseProvider, UnknownDatabaseProvider>();
                break;
        }

        return new(builder.Options, shape);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var schema = Database.ProviderName!.Contains("MySql", StringComparison.OrdinalIgnoreCase) ? null : "Tenant";
        if (shape == "shared-type")
        {
            foreach (var name in new[] { "First", "Second" })
            {
                modelBuilder.SharedTypeEntity<Dictionary<string, object>>(name, entity =>
                {
                    entity.IndexerProperty<int>("Id");
                    entity.HasKey("Id");
                    entity.ToTable(name, schema);
                });
            }
            return;
        }

        var item = modelBuilder.Entity<Item>();
        item.HasQueryFilter(entity => entity.Id > 0);
        item.ToTable("Items", schema);
        switch (shape)
        {
            case "escaped":
                item.ToTable("Items]\"`", schema);
                break;
            case "tph":
                modelBuilder.Entity<DerivedItem>();
                break;
            case "tpt":
                item.UseTptMappingStrategy();
                modelBuilder.Entity<DerivedItem>().ToTable("DerivedItems", schema);
                break;
            case "tpc":
                item.UseTpcMappingStrategy();
                modelBuilder.Entity<DerivedItem>().ToTable("DerivedItems", schema);
                break;
            case "split":
                item.SplitToTable("ItemDetails", schema, table => table.Property(entity => entity.Name));
                break;
            case "owned":
                item.OwnsOne(entity => entity.Details);
                return;
            case "view":
                item.ToTable((string?)null);
                item.ToView("ItemView", schema);
                break;
            case "table-sharing":
                modelBuilder.Entity<OtherItem>().ToTable("Items", schema);
                item.HasOne<OtherItem>().WithOne().HasForeignKey<OtherItem>(entity => entity.Id);
                break;
        }
        item.Ignore(entity => entity.Details);
    }

    internal class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Value { get; set; }
        public Details? Details { get; set; }
    }

    internal sealed class DerivedItem : Item;
    internal sealed class OtherItem
    {
        public int Id { get; set; }
    }

    internal sealed class Details
    {
        public string? Value { get; set; }
    }

    private sealed class ShapeModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
        {
            return (context.GetType(), ((TruncateTestContext)context).Shape, designTime);
        }
    }

    private sealed class UnknownDatabaseProvider : IDatabaseProvider
    {
        public string Name => "Unknown.Provider";
        public string? Version => null;

        public bool IsConfigured(IDbContextOptions options) => true;
    }

    internal sealed class CommandRecorder : DbConnectionInterceptor, IDbCommandInterceptor
    {
        internal List<string> Commands { get; } = [];
        internal CancellationToken CommandToken { get; private set; }
        internal int OpenCount { get; private set; }

        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection, ConnectionEventData eventData, InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            OpenCount++;
            return ValueTask.FromResult(InterceptionResult.Suppress());
        }

        public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);
            CommandToken = cancellationToken;
            return ValueTask.FromResult(InterceptionResult<int>.SuppressWithResult(-1));
        }
    }
}
