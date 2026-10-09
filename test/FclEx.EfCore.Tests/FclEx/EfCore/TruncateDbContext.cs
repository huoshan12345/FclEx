namespace FclEx.EfCore;

/// <summary>A minimal model used only for isolated truncate tests.</summary>
internal sealed class TruncateDbContext(
    DbDriver driver, string connectionString, string? schema = null, DbContextOptions<TestDbContext>? options = null)
    : TestDbContext(driver, connectionString, schema, options)
{
    public DbSet<TruncateRow> TruncateRow { get; set; }
    public DbSet<TruncateOtherRow> TruncateOtherRow { get; set; }
    public DbSet<TruncateManualRow> TruncateManualRow { get; set; }
    public DbSet<TruncateKeylessRow> TruncateKeylessRow { get; set; }
    public DbSet<TruncateParent> TruncateParent { get; set; }
    public DbSet<TruncateChild> TruncateChild { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes().ToArray())
            builder.Ignore(entity.ClrType);

        builder.Entity<TruncateRow>().ToTable(nameof(TruncateRow), Schema);
        builder.Entity<TruncateOtherRow>().ToTable(nameof(TruncateOtherRow), Schema);
        builder.Entity<TruncateManualRow>().ToTable(nameof(TruncateManualRow), Schema);
        builder.Entity<TruncateKeylessRow>().HasNoKey().ToTable(nameof(TruncateKeylessRow), Schema);
        builder.Entity<TruncateParent>().ToTable(nameof(TruncateParent), Schema);
        builder.Entity<TruncateChild>().ToTable(nameof(TruncateChild), Schema);
        builder.Entity<TruncateChild>().HasOne<TruncateParent>().WithMany()
            .HasForeignKey(row => row.ParentId).OnDelete(DeleteBehavior.Cascade);
    }
}
