namespace FclEx.EfCore;

public class SchemaDbContext : DbContext, IHasSchema
{
    public string? Schema { get; }

    protected SchemaDbContext(string? schema)
    {
        Schema = schema;
    }

    public SchemaDbContext(DbContextOptions options, string? schema) : base(options)
    {
        Schema = schema;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);
        // Needed so EF won't cache the tenant schema
        builder.ReplaceService<IModelCacheKeyFactory, SchemaModelCacheKeyFactory>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
    }
}