namespace FclEx.EfCore.Extensions;

internal sealed class ApplyKeyTestContext : DbContext
{
    public ApplyKeyTestContext()
        : base(new DbContextOptionsBuilder<ApplyKeyTestContext>().UseSqlite("Data Source=:memory:").Options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(builder =>
        {
            builder.HasKey(entity => entity.Id);
            builder.Property(entity => entity.Id).ValueGeneratedOnAdd();
            builder.HasAlternateKey(entity => entity.Code);
        });
        modelBuilder.Entity<CompositeItem>().HasKey(entity => new { entity.TenantId, entity.Code });
        modelBuilder.Entity<FieldItem>(builder =>
        {
            builder.Property<int>(nameof(FieldItem.Id)).ValueGeneratedNever();
            builder.HasKey(nameof(FieldItem.Id));
        });
        modelBuilder.Entity<ShadowItem>(builder =>
        {
            builder.Property<int>("Id").ValueGeneratedNever();
            builder.HasKey("Id");
        });
        modelBuilder.Entity<KeylessItem>().HasNoKey();
    }

    internal class Item
    {
        public int Id { get; set; }
        public string Code { get; set; } = "code";
        public string Name { get; set; } = "";
    }

    internal sealed class CompositeItem
    {
        public int TenantId { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    internal sealed class FieldItem
    {
        public int Id;
        public string Name { get; set; } = "";
    }

    internal sealed class ShadowItem
    {
        public string Name { get; set; } = "";
    }

    internal sealed class KeylessItem
    {
        public string Name { get; set; } = "";
    }
}
