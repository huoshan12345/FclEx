using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.Data;

// EfCore is used for helping us to do tests
public class GlobalDbContext : DbContext
{
    public const string ConnectionString = "Data Source=./FclEx.Benchmarks.sqlite;";

    public DbSet<EntityWithAutoKey> EntityWithAutoKeys { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var e = modelBuilder.Entity<EntityWithAutoKey>();
        e.ToTable(nameof(EntityWithAutoKey));
        e.HasKey(m => m.Id);
        e.Property(m => m.Id).ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
    }
}