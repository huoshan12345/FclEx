namespace FclEx.EfCore;

public class DbContextWithSchema : DbContext, IHasSchema
{
    public string? Schema { get; }

    protected DbContextWithSchema(string? schema)
    {
        Schema = schema;
    }

    public DbContextWithSchema(DbContextOptions options, string? schema) : base(options)
    {
        Schema = schema;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        // Needed so EF won't cache the tenant schema
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, ModelCacheKeyWithSchemaFactory>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
    }
}