using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FclEx.Data;

// EfCore is used for helping us to do tests
public class GlobalDbContext : DbContext
{
    public const string LocalPostgresqlConnectionString = "Server=localhost;Database=fclex-test-efcore;Port=5432;User Id=postgres;Password=111111";

    public DbSet<EntityWithAutoKey> EntityWithAutoKeys { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(LocalPostgresqlConnectionString);
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