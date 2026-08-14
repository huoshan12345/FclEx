namespace FclEx.EfCore;

/// <summary>
/// A <see cref="DbContext"/> base class whose default schema can be selected per context instance.
/// </summary>
/// <remarks>
/// The context replaces EF Core's model cache key factory so models created for different schemas are cached separately.
/// </remarks>
public class SchemaDbContext : DbContext, IHasSchema
{
    /// <inheritdoc />
    public string? Schema { get; }

    /// <summary>
    /// Initializes a context that configures its provider in <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/>.
    /// </summary>
    /// <param name="schema">The default schema, or <see langword="null"/> to use the provider default.</param>
    protected SchemaDbContext(string? schema)
    {
        Schema = schema;
    }

    /// <summary>
    /// Initializes a context with the supplied options and default schema.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    /// <param name="schema">The default schema, or <see langword="null"/> to use the provider default.</param>
    public SchemaDbContext(DbContextOptions options, string? schema) : base(options)
    {
        Schema = schema;
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        base.OnConfiguring(builder);
        // Needed so EF won't cache the tenant schema
        builder.ReplaceService<IModelCacheKeyFactory, SchemaModelCacheKeyFactory>();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
    }
}
