namespace FclEx.EfCore;

/// <summary>
/// Exposes the database schema that contributes to an Entity Framework Core model.
/// </summary>
/// <remarks>
/// <see cref="SchemaModelCacheKeyFactory"/> reads this value so contexts of the same CLR type can cache separate models for separate schemas.
/// </remarks>
public interface IHasSchema
{
    /// <summary>
    /// Gets the schema used by the context, or <see langword="null"/> to use the provider's default schema.
    /// </summary>
    string? Schema { get; }
}
